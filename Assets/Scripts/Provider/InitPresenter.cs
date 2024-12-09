#nullable enable
using Unity.Logging;
using Unity.Logging.Sinks;
using VContainer;
using Logger = Unity.Logging.Logger;

namespace Provider
{
    public class InitPresenter
    {
        [Inject]
        public InitPresenter()
        {
            LoggerInit();
        }

        private void LoggerInit()
        {
            Log.Logger = new Logger(Configuration());
            Log.Debug("[Game]Init Logger");
        }
        private static LoggerConfig Configuration()
        {
#if UNITY_EDITOR
            return new LoggerConfig()
                .SyncMode.FullSync()
                .WriteTo.UnityDebugLog(
                    minLevel: LogLevel.Info,
                    captureStackTrace: true);
#elif DEBUG
            return new LoggerConfig()
                .SyncMode.FatalIsSync()
                //.RedirectUnityLogs(log:true)
                .WriteTo.File(
                    $"{Application.persistentDataPath}/Logs/logging_dev/client_dev_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log",
                    minLevel: LogLevel.Debug,
                    captureStackTrace: true,
                    outputTemplate: "{Timestamp} [{Level}] {Message}{NewLine}{Stacktrace}");
#else
            return new LoggerConfig()
                .SyncMode.FatalIsSync()
                //.RedirectUnityLogs(log:true)
                .WriteTo.File(
                    $"{Application.persistentDataPath}/Logs/logging/client_release_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log",
                    minLevel: LogLevel.Info,
                    captureStackTrace: false,
                    outputTemplate: "{Timestamp} [{Level}] {Message}");
#endif
        }
    }
}