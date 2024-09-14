using Domain.Service.Logs;
using IngameDebugConsole;
using UnityEngine;
using VContainer;

namespace Provider
{
    public class DebugCommands
    {
        [Inject]
        public DebugCommands()
        {
            DebugLogConsole.AddCommandInstance("test", "テスト", "Test", this);
            DebugLogConsole.AddCommandInstance("log", "画面にログを出力します。", "Log", this);
        }

        private void Test(string message)
        {
            Debug.Log(message);
        }

        private void Log(string log)
        {
            GameLog.Add(log);
        }
    }
}