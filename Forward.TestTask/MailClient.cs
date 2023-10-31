using MailKit.Net.Imap;
using MailKit.Security;
using MailKit;
using System.Security.Authentication;
using Microsoft.Extensions.Configuration;

namespace Forward.TestTask;

public class MailClient
{
	private readonly string _onlyRecieveMail;
	//private readonly ulong _controllerID;
	private readonly string _taskPrefix;

	private readonly string _host;
	private readonly int _port;
	private readonly string _login;
	private readonly string _pass;
	private readonly int _interval;

	private readonly Dictionary<UniqueId, IMessageSummary> _messages;

	private readonly CancellationTokenSource _cancel;
	private readonly ImapClient _client;
	private CancellationTokenSource _done;
	private bool _messagesArrived;
	private readonly string _mailData;
	

	public MailClient(IConfiguration config, string taskPrefix, bool isOnlyReciveMessage)
	{
		var delayBetweenCheck = config.GetRequiredSection("Delay").Value;
		if(!int.TryParse(delayBetweenCheck, out _interval)) _interval = 10;

		if (isOnlyReciveMessage)
		{
			_onlyRecieveMail = settings;
		}

		_mailData = settings;

		var sslType = config.GetRequiredSection(@"sslProtocol").Value;
		_host = config.GetRequiredSection("Adress").Value;

		var configPortValue = config.GetRequiredSection("Port").Value;
		_port = Convert.ToInt32(configPortValue);

		_login = config.GetRequiredSection("Login").Value;
		_pass = config.GetRequiredSection("Password").Value;

		var noCertCheckConfigValue = config.GetRequiredSection("CertCheck").Value;
		var noCertCheck = bool.Parse(noCertCheckConfigValue);

		_taskPrefix = taskPrefix;

		SslProtocols sslProtocols;
		switch (sslType)
		{
			case "2":
				{
					sslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

					break;
				}
			case "3":
				{
					sslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13 | SslProtocols.Tls;

					break;

				}
			default:
				{
					sslProtocols = SslProtocols.None;

					break;
				}
		}

		_cancel = new CancellationTokenSource();
		_client = new ImapClient
		{
			SslProtocols = sslProtocols,
			CheckCertificateRevocation = noCertCheck
		};
		_messages = new Dictionary<UniqueId, IMessageSummary>();
	}

	public void Start()
	{
		Task.Run(async () =>
		{
			try
			{
				await StartAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		});
	}

	public void Stop()
	{
		_cancel.Cancel();
	}

	private async Task StartAsync()
	{
		try
		{
			await ReconnectAsync();
			await FetchMessagesAsync();
		}
		catch (Exception ex)
		{
			if (ex is OperationCanceledException)
			{
				_client.Disconnect(true);
				return;
			}

			throw;
		}

		var inbox = _client.Inbox;

		inbox.CountChanged += Inbox_CountChanged;
		inbox.MessageExpunged += Inbox_MessageExpunged;

		await IdleAsync();

		inbox.CountChanged -= Inbox_CountChanged;
		inbox.MessageExpunged -= Inbox_MessageExpunged;

		await _client.DisconnectAsync(true);
	}

	private void Inbox_MessageExpunged(object sender, MessageEventArgs e)
	{
		if (e.Index < _messages.Count)
		{
			if (e.UniqueId != null)
			{
				_messages.Remove(e.UniqueId.Value);
			}
		}
	}

	private void Inbox_CountChanged(object sender, EventArgs e)
	{
		var folder = (ImapFolder)sender;

		if (folder.Count > _messages.Count)
		{
			_messagesArrived = true;
			_done?.Cancel();
		}
	}

	private async Task IdleAsync()
	{
		do
		{
			try
			{
				await WaitForMessagesAsync();

				if (_messagesArrived)
				{
					await FetchMessagesAsync();
					_messagesArrived = false;
				}
			}
			catch (Exception ex)
			{
				if (ex is OperationCanceledException)
				{
					//_client.Disconnect(true);
					break;
				}

				throw;
			}
		}
		while (!_cancel.IsCancellationRequested);
	}

	private async Task FetchMessagesAsync()
	{
		IList<IMessageSummary> fetched;
		do
		{
			try
			{
				var startIndex = _messages.Count;
				fetched = await _client.Inbox.FetchAsync(startIndex, -1, MessageSummaryItems.Flags | MessageSummaryItems.UniqueId, _cancel.Token);
				break;
			}
			catch (Exception ex)
			{
				if (ex is ImapProtocolException ||
					ex is IOException)
				{
					await ReconnectAsync();
					continue;
				}

				throw;
			}
		}
		while (true);

		using (var context = new ContextRep().GetContext())
		{
			var mainContext = new MainContext(context);
			foreach (var messageSummary in fetched)
			{
				_messages.Add(messageSummary.UniqueId, messageSummary);

				ParseMessage(_mailData, messageSummary, _taskPrefix, _client.Inbox, mainContext);
			}
		}
	}

	private async Task WaitForMessagesAsync()
	{
		do
		{
			try
			{
				using (_done = new CancellationTokenSource(new TimeSpan(0, 9, 0)))
				{
					await _client.IdleAsync(_done.Token, _cancel.Token);
				}

				break;
			}
			catch (Exception ex)
			{
				if (ex is ImapProtocolException ||
					ex is IOException)
				{
					await ReconnectAsync();
					continue;
				}

				throw;
			}
		}
		while (true);
	}

	private async Task ReconnectAsync()
	{
		if (!_client.IsConnected)
		{
			await _client.ConnectAsync(_host, _port, SecureSocketOptions.Auto, _cancel.Token);
		}

		if (!_client.IsAuthenticated)
		{
			await _client.AuthenticateAsync(_login, _pass, _cancel.Token);
			await _client.Inbox.OpenAsync(FolderAccess.ReadOnly, _cancel.Token);
		}
	}

	public void Dispose()
	{
		_cancel?.Dispose();

		_client?.Dispose();

		_done?.Dispose();
	}
}