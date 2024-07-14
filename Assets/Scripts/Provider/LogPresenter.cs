#nullable enable
using System;
using Domain.Model.Setting;
using Domain.Service.Logs;
using Model.Game;
using R3;
using VContainer;
using View.UI;

namespace Provider
{
    public class LogPresenter
    {
        [Inject]
        public LogPresenter(LogView logView)
        {
            Settings.LogShownMilliSeconds.Subscribe(logView.SetLogShownMilliSeconds);
            GameLog.OnLogOutput.Subscribe(logView.AddLog);
        }
    }
}