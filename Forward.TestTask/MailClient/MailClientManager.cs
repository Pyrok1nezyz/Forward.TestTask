using Forward.TestTask.DAL;
using Forward.TestTask.Domain.Entitys;
using MailKit.Net.Imap;
using Microsoft.Extensions.Configuration;

namespace Forward.TestTask.MailClient;

public class MailClientManager
{
    private Dictionary<ulong, MailClient> _watchers;
    private readonly UnitOfWork _unitOfWork;
    private int _interval;

    public MailClientManager(IConfiguration config, bool isNeedToAddTestMail)
    {
	    _unitOfWork = new UnitOfWork(config);
        _watchers = new Dictionary<ulong, MailClient>();

        var configDelayValue = config.GetSection("Delay").Value;
        int.TryParse(configDelayValue, out _interval);

        if(isNeedToAddTestMail)
	        TryAddTestEmail(config);
    }

    private void TryAddTestEmail(IConfiguration config)
    {
	    var login = config.GetValue<string>("Login");
	    var password = config.GetValue<string>("Password");
	    var address = config.GetSection("Address").Value;

	    var mailBoxSettings = new MailBoxSettings
	    {
		    Adress = address ?? throw new Exception("Не удалось получить адресс из конфига"),
		    CertCheck = true,
		    Id = 99999,
		    Login = login ?? throw new Exception("Не удалось получить логин из конфига"),
		    Password = password,
		    Port = 993,
	    };

		if (IsCanAddPrivateEmailBox(mailBoxSettings))
            AddPrivateEmail(mailBoxSettings);
    }

    private bool IsCanAddPrivateEmailBox(MailBoxSettings settings)
    {
        using (var client = new ImapClient())
        {
	       client.Connect(settings.Adress, 993, true);
	       return client.IsConnected;
        }
    }

    private void AddPrivateEmail(MailBoxSettings settings)
    {
	    var client = GetMailClient(settings, false);
        _watchers.Add(99999, client);
    }

    public void InitWatchers(bool needRestart)
    {
        UpdateWatchers(needRestart);
    }

    public void UpdateWatchers(bool needRestart, IEnumerable<MailBoxSettings>? mailBoxes = null)
    {
        var listMailBoxes = _unitOfWork.MailBoxRepository.GetAll().Result;

        if (_watchers == null)
        {
            _watchers = new Dictionary<ulong, MailClient>();
        }

        foreach (var boxSettings in listMailBoxes)
        {
            if (!_watchers.TryGetValue(boxSettings.Id, out var watcher))
            {
                watcher = GetMailClient(boxSettings, true);
                _watchers.Add(boxSettings.Id, watcher);
            }

            if (needRestart)
            {
                watcher.Stop();
            }

            watcher.Start();
        }

        if (mailBoxes != null)
	        foreach (var mailBox in mailBoxes)
	        {
		        UpdateMailClient(mailBox, needRestart);
	        }
    }

    public void UpdateMailClient(ulong id, MailBoxSettings settings, bool needRestart)
    {
        MailClient? watcher;
        if (_watchers.ContainsKey(id))
        {
            if (_watchers.TryGetValue(id, out watcher))
            {
                watcher.Stop();
            }

            return;
        }

        if (!_watchers.TryGetValue(id, out watcher))
        {
            watcher = GetMailClient(settings, true);
            _watchers.Add(id, watcher);
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
            watcher = GetMailClient(mailSettings, true);
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

    private MailClient GetMailClient(MailBoxSettings settings, bool isOnlyReciveMail = false)
    {
	    var template = _unitOfWork
		    .MailTemplateRepository
		    .GetByMailBoxId(settings.Id)
		    .Result;
        return new MailClient(settings, template, _unitOfWork, _interval);
    }
}