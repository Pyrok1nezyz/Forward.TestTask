using Forward.TestTask.DAL.Entitys;
using Forward.TestTask.DAL.Repositorys;
using Forward.TestTask.DAL.Repositorys.Classes;
using MailKit.Net.Imap;
using Microsoft.Extensions.Configuration;

namespace Forward.TestTask.MailClient;

public class MailClientManager
{
    private Dictionary<ulong, MailClient> _watchers;
    private MailClient _client;
    private static UnitOfWork _unitOfWork;
    private static int _interval;

    public MailClientManager(IConfiguration config)
    {
        _unitOfWork = new UnitOfWork();

        var configDelayValue = config.GetSection("Delay").Value;
        int.TryParse(configDelayValue, out _interval);

        TryAddTestEmail(config);
    }

    private void TryAddTestEmail(IConfiguration config)
    {
	    var login = config.GetValue<string>("Login");
	    var password = config.GetValue<string>("Password");
	    var address = config.GetSection("Address").Value;

	    var mailBoxSettings = new MailBoxSettings
	    {
		    Adress = address,
		    CertCheck = true,
		    Id = 99999,
		    Login = login,
		    Password = password,
		    Port = 993,
		    SslProtocol = 3
	    };

		if (IsCanAddPrivateEmailBox(config, mailBoxSettings))
            AddPrivateEmail(mailBoxSettings);
    }

    private bool IsCanAddPrivateEmailBox(IConfiguration config, MailBoxSettings settings)
    {
        using (var client = new ImapClient())
        {
	       client.Connect(settings.Adress, 993, true);
	       return client.IsConnected;
        }
    }

    private void AddPrivateEmail(MailBoxSettings settings)
    {
	    var client = GetMailClient(settings, _interval);
        _watchers.Add(99999, client);
    }

    public void InitWatchers(bool needRestart)
    {
        UpdateWatchers(needRestart);
    }

    public void UpdateWatchers(bool needRestart, IEnumerable<MailBoxSettings> mailBoxes = null)
    {
        //var mainContext = Global.Container.GetInstance<MainContext>();

        var listMailBoxes = _unitOfWork.mailBoxRepository.GetAll().Result;

        if (_watchers == null)
        {
            _watchers = new Dictionary<ulong, MailClient>();
        }

        foreach (var boxSettings in listMailBoxes)
        {
            if (!_watchers.TryGetValue(boxSettings.Id, out var watcher))
            {
                watcher = GetMailClient(boxSettings, delay: _interval);
                _watchers.Add(boxSettings.Id, watcher);
            }

            if (needRestart)
            {
                watcher.Stop();
            }

            watcher.Start();
        }

        foreach (var mailBox in mailBoxes)
        {
            UpdateMailClient(mailBox, needRestart);
        }
    }

    public void UpdateMailClient(ulong Id, MailBoxSettings settings, bool needRestart)
    {
        MailClient watcher;
        if (_watchers.ContainsKey(Id))
        {
            if (_watchers.TryGetValue(Id, out watcher))
            {
                watcher.Stop();
            }

            return;
        }

        if (!_watchers.TryGetValue(Id, out watcher))
        {
            watcher = GetMailClient(settings, _interval, true);
            _watchers.Add(Id, watcher);
        }

        if (needRestart)
        {
            watcher.Stop();
        }

        watcher.Start();
    }
    public void UpdateMailClient(MailBoxSettings mailSettings, bool needRestart)
    {
        if (mailSettings is null) return;

        if (!_watchers.TryGetValue(0, out var watcher))
        {
            watcher = GetMailClient(mailSettings, _interval, true);
            _watchers.Add(0, watcher);
        }

        if (needRestart)
        {
            watcher.Stop();
        }

        watcher.Start();
    }

    public void Stop()
    {
        foreach (var watcher in _watchers)
        {
            watcher.Value.Stop();
            watcher.Value.Dispose();
        }

        _watchers.Clear();
    }

    private static MailClient GetMailClient(MailBoxSettings settings, int delay, bool isOnlyReciveMail = false)
    {
        //TODO
        return new MailClient(settings, _interval, _unitOfWork, isOnlyReciveMail);
    }
}