using MailKit.Net.Imap;
using MailKit.Security;
using MailKit;
using System.Security.Authentication;
using System.Text;
using Forward.TestTask.DAL.Entitys;
using Forward.TestTask.DAL.Repositorys;
using Forward.TestTask.DAL.Repositorys.Classes;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Forward.TestTask.MailClient;

public class MailClient
{
    private readonly string _onlyRecieveMail;
    //private readonly ulong _controllerID;

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
    private UnitOfWork _unitOfWork;
    //private readonly string _mailData;


    public MailClient(MailBoxSettings settings, int minDelay, UnitOfWork unitOfWork, bool isOnlyReciveMessage)
    {
        _unitOfWork = unitOfWork;
        _interval = minDelay;

        if (isOnlyReciveMessage)
        {
            _onlyRecieveMail = settings.ToString();
        }

        //_mailData = settings;

        var sslType = settings.SslProtocol;
        _host = settings.Adress;

        _port = settings.Port;

        _login = settings.Login;
        _pass = settings.Password;

        var noCertCheck = settings.CertCheck;

        SslProtocols sslProtocols;
        switch (sslType)
        {
            case 2:
                {
                    sslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                    break;
                }
            case 3:
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

		foreach (var messageSummary in fetched)
		{
			_messages.Add(messageSummary.UniqueId, messageSummary);

			ParseMessage(messageSummary);
		}

	}

	private void ParseMessage(IMessageSummary summary)
	{
		//TODO
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