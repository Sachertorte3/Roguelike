#nullable enable
using Model.Game;
using Unity.Logging;
using Unity.Logging.Sinks;
using VContainer;
using Logger = Unity.Logging.Logger;

namespace Provider
{
    public class Presenter
    {
        [Inject]
        public Presenter(GameManager gameManager, SynchronizedIconEntityView _)
        {
            LoggerInit();
            gameManager.Load();
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
                    minLevel: LogLevel.Info,
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