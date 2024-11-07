#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Game;
using R3;
using Unity.Logging;
using Unity.Logging.Sinks;
using UnityEngine;
using VContainer;
using View.UI;
using Logger = Unity.Logging.Logger;

namespace Provider
{
    public class Presenter
    {
        [Inject]
        public Presenter(GameManager gameManager, SynchronizedIconEntityView _, SynchronizedThrowAnimationEntityView _2,
            SynchronizedFireEntityView _3, MenuController menuController)
        {
            LoggerInit();
            gameManager.State.Subscribe(state =>
            {
                switch (state)
                {
                    case GameState.Title:
                        Log.Debug("[Game]Change to title scene.");
                        gameManager.Title().Forget();
                        menuController.TitleMenu();
                        break;
                    case GameState.Dungeon:
                        Log.Debug("[Game]Change to dungeon scene.");
                        menuController.DungeonMenu();
                        break;
                }
            });
        }

        private void LoggerInit()
        {
            Log.Logger = new Logger(
#if UNITY_EDITOR
                EditorConfiguration()
#elif DEBUG
                DevelopmentConfiguration()
#else
                ReleaseConfiguration()
#endif
            );
            Log.Debug("[Game]Init Logger");
        }

        private static LoggerConfig EditorConfiguration()
        {
            return new LoggerConfig()
                .SyncMode.FullSync()
                .WriteTo.UnityDebugLog(
                    minLevel: LogLevel.Info,
                    captureStackTrace: true);
        }

        private static LoggerConfig DevelopmentConfiguration()
        {
            return new LoggerConfig()
                .SyncMode.FatalIsSync()
                //.RedirectUnityLogs(log:true)
                .WriteTo.File(
                    $"{Application.persistentDataPath}/Logs/logging_dev/client_dev_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log",
                    minLevel: LogLevel.Debug,
                    captureStackTrace: true,
                    outputTemplate: "{Timestamp} [{Level}] {Message}{NewLine}{Stacktrace}");
        }

        private static LoggerConfig ReleaseConfiguration()
        {
            return new LoggerConfig()
                .SyncMode.FatalIsSync()
                //.RedirectUnityLogs(log:true)
                .WriteTo.File(
                    $"{Application.persistentDataPath}/Logs/logging/client_release_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log",
                    minLevel: LogLevel.Info,
                    captureStackTrace: false,
                    outputTemplate: "{Timestamp} [{Level}] {Message}");
        }
    }
}