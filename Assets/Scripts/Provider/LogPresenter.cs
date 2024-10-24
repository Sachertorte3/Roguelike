#nullable enable
using Domain.Model.Setting;
using Domain.Service.Logs;
using R3;
using VContainer;
using View.UI;

namespace Provider
{
    public class LogPresenter
    {
        private CompositeDisposable _disposables = new();

        [Inject]
        public LogPresenter(LogView logView)
        {
            _disposables.Add(Settings.LogShownMilliSeconds.Subscribe(logView.SetLogShownMilliSeconds));
            _disposables.Add(GameLog.OnLogOutput.Subscribe(logView.AddLog));
            _disposables.Add(GameLog.OnClear.Subscribe(_ => logView.Clear()));
        }

        ~LogPresenter()
        {
            _disposables.Dispose();
        }
    }
}