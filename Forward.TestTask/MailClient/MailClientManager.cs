using Forward.TestTask.DAL.Entitys;
using Forward.TestTask.DAL.Repositorys;
using Forward.TestTask.DAL.Repositorys.Classes;
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
                watcher = GetHelpDeskWatcher(boxSettings, delay: _interval);
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
            UpdateWatcher(mailBox, needRestart);
        }
    }

    public void UpdateWatcher(ulong Id, MailBoxSettings settings, bool needRestart)
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
            watcher = GetHelpDeskWatcher(settings, _interval, true);
            _watchers.Add(Id, watcher);
        }

        if (needRestart)
        {
            watcher.Stop();
        }

        watcher.Start();
    }
    public void UpdateWatcher(MailBoxSettings mailSettings, bool needRestart)
    {
        if (mailSettings is null) return;

        if (!_watchers.TryGetValue(0, out var watcher))
        {
            watcher = GetHelpDeskWatcher(mailSettings, _interval, true);
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

    private static MailClient GetHelpDeskWatcher(MailBoxSettings settings, int delay, bool isOnlyReciveMail = false)
    {
        //TODO
        return new MailClient(settings, _interval, _unitOfWork, isOnlyReciveMail);
    }
}