using MailKit.Net.Imap;
using MailKit.Security;
using MailKit;
using Forward.TestTask.DAL;
using Forward.TestTask.Domain.Entitys;
using Forward.TestTask.Parsers;
using MimeKit;

namespace Forward.TestTask.MailClient;

public class MailClient
{
	//private readonly ulong _controllerID;
    private readonly string _host;
    private readonly int _port;
    private readonly string _login;
    private readonly string? _pass;
    private readonly int _interval;
    private readonly MailTemplate _template;

    private readonly Dictionary<UniqueId, IMessageSummary> _messages;

    private readonly CancellationTokenSource _cancel;
    private readonly ImapClient _client;
    private bool _messagesArrived;
    private readonly UnitOfWork _unitOfWork;
    private CancellationTokenSource _done;
    //private readonly string _mailData;

    public MailClient(MailBoxSettings settings, MailTemplate mailTemplate, UnitOfWork unitOfWork, int minDelay)
    {
        _unitOfWork = unitOfWork;
        _interval = minDelay;
        _template = mailTemplate;

        _host = settings.Adress;
        _port = settings.Port;
        _login = settings.Login;
        _pass = settings.Password;
        var certCheck = settings.CertCheck ??= false;

        _cancel = new CancellationTokenSource();
        _done = new CancellationTokenSource();
        _client = new ImapClient
        {

            CheckCertificateRevocation = certCheck
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

    private void Inbox_MessageExpunged(object? sender, MessageEventArgs e)
    {
        if (e.Index < _messages.Count)
        {
            if (e.UniqueId != null)
            {
                _messages.Remove(e.UniqueId.Value);
            }
        }
    }

    private void Inbox_CountChanged(object? sender, EventArgs e)
    {
	    if (sender == null) throw new ArgumentNullException(nameof(sender));

	    var folder = (ImapFolder)sender;

        if (folder.Count > _messages.Count)
        {
            _messagesArrived = true;
            _done.Cancel();
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
        var parser = new MailTemplateParser(_template, _unitOfWork);
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

			var message = await ParseMessage(messageSummary);

			await parser.TryAddMessageToDb(message);

			DeleteMessageFromMailBox(messageSummary);

            if(_template.IsDelete) DeleteMessageFromMailBox(messageSummary);
		}
	}

    private void DeleteMessageFromMailBox(IMessageSummary summary)
    {
		_client.Inbox.AddFlags(summary.UniqueId, MessageFlags.Deleted, false);
	}

    private async Task<MimeMessage> ParseMessage(IMessageSummary summary)
	{
		//TODO MailMessageParser();
        var message = await _client.Inbox.GetMessageAsync(summary.UniqueId);
        return message;
	}

    private async Task WaitForMessagesAsync()
    {
        do
        {
            try
            {
	            await Task.Delay(_interval * 60 * 1000);

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
        _cancel.Dispose();

        _client.Dispose();

        _done.Dispose();
    }
}