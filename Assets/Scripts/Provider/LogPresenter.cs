#nullable enable
using System;
using Model.Domain.Logs;
using Utilities;
using VContainer;
using View.UI;

namespace Provider
{
    public class LogPresenter
    {
        private IDisposable _disposable;

        [Inject]
        public LogPresenter(LogView logView)
        {
            _disposable = GameLog.Logs.SubscribeToAll(logView.AddLog);
        }

        ~LogPresenter()
        {
            _disposable.Dispose();
        }
    }
}