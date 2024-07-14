#nullable enable
using System;
using Domain.Model.Setting;
using Domain.Service.Logs;
using R3;
using VContainer;
using View.UI;

namespace Provider
{
    public class LogPresenter
    {
        private CompositeDisposable _disposable = new();

        [Inject]
        public LogPresenter(LogView logView)
        {
            _disposable.Add(Settings.LogShownMilliSeconds.Subscribe(logView.SetLogShownMilliSeconds));
            _disposable.Add(GameLog.OnLogOutput.Subscribe(logView.AddLog));
        }

        ~LogPresenter()
        {
            _disposable.Dispose();
        }
    }
}