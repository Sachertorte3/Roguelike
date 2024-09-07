#nullable enable
using Model.Game;
using Unity.Logging;
using Unity.Logging.Sinks;
using VContainer;
using R3;
using Logger = Unity.Logging.Logger;
using View.UI;
using Cysharp.Threading.Tasks;

namespace Provider
{
    public class Presenter
    {
        [Inject]
        public Presenter(GameManager gameManager, SynchronizedIconEntityView _, SynchronizedThrowAnimationEntityView _2, MenuController menuController)
        {
            LoggerInit();
            gameManager.State.Subscribe(state =>
            {
                switch (state)
                {
                    case GameState.Title:
                        Log.Debug("Title");
                        break;
                    case GameState.Dungeon:
                        Log.Debug("Dungeon");
                        menuController.DungeonMenu();
                        break;
                }
            });
            gameManager.Title().Forget();
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
            Log.Debug("Init Logger");
        }
        private static LoggerConfig EditorConfiguration()
            => new LoggerConfig()
                .SyncMode.FullSync()
                //.RedirectUnityLogs(log:true)
                .WriteTo.UnityEditorConsole(
                    minLevel: LogLevel.Debug,
                    captureStackTrace: true);

        private static LoggerConfig DevelopmentConfiguration()
            => new LoggerConfig()
                .SyncMode.FatalIsSync()
                //.RedirectUnityLogs(log:true)
                .WriteTo.File(
                    absFileName: $"{UnityEngine.Application.persistentDataPath}/Logs/logging_dev/client_dev_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log",
                    minLevel: LogLevel.Debug,
                    captureStackTrace: true,
                    outputTemplate: "{Timestamp} [{Level}] {Message}{NewLine}{Stacktrace}");

        private static LoggerConfig ReleaseConfiguration()
            => new LoggerConfig()
                .SyncMode.FatalIsSync()
                //.RedirectUnityLogs(log:true)
                .WriteTo.File(
                    absFileName: $"{UnityEngine.Application.persistentDataPath}/Logs/logging/client_release_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log",
                    minLevel: LogLevel.Info,
                    captureStackTrace: false,
                    outputTemplate: "{Timestamp} [{Level}] {Message}");
    }
}