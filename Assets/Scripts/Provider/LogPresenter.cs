#nullable enable
using Model;
using Model.Logs;
using R3;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;
using View;
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