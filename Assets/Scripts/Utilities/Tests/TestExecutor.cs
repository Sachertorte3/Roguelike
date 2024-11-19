using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Utilities.Tests
{
    public class TestExecutor
    {
        //[DidReloadScripts]
        public static void RunTests()
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();

            // 実行するテストを指定
            var filter = new Filter
            {
                testMode = TestMode.EditMode
            };

            testRunnerApi.RegisterCallbacks(new MyTestCallbacks());

            // 実行
            testRunnerApi.Execute(new ExecutionSettings(filter));
        }

        // 結果を取り出すためのコールバック処理を定義
        private class MyTestCallbacks : ICallbacks
        {
            private StackTraceLogType _tmpStackTraceLogType;

            /// <summary>
            /// テスト全体を実行開始する前に呼ばれる
            /// </summary>
            public void RunStarted(ITestAdaptor testsToRun)
            {
                // スタックトレースを一時的に無効化
                _tmpStackTraceLogType = Application.GetStackTraceLogType(LogType.Log);
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

                Debug.Log("Test Started.");
            }

            /// <summary>
            /// すべてのテストが完了した後に呼ばれる
            /// </summary>
            public void RunFinished(ITestResultAdaptor result)
            {
                Debug.Log($"All test finished: {result.TestStatus}");
                Application.SetStackTraceLogType(LogType.Log, _tmpStackTraceLogType);

                // 最終結果を表示
                Debug.Log($"Passed:{result.PassCount} Skipped:{result.SkipCount} Failed:{result.FailCount}");

                // BathMode時はテスト完了後に終了する
                if (Application.isBatchMode)
                {
                    // Failedがあったなら異常終了
                    // 失敗ゼロの場合は正常終了
                    EditorApplication.Exit(result.FailCount == 0 ? 0 : 1);
                }
            }

            /// <summary>
            /// 各テストが実行される前に呼ばれる
            /// </summary>
            public void TestStarted(ITestAdaptor test)
            {
                Debug.unityLogger.logEnabled = false;
            }

            /// <summary>
            /// 各テストが実行された後に呼ばれる
            /// </summary>
            public void TestFinished(ITestResultAdaptor result)
            {
                Debug.unityLogger.logEnabled = true;

                // ITestResultAdaptorはツリー構造であり、dll、クラス、各メソッドすべてがごちゃまぜになって結果が届く
                // そのまま出力すると重複した結果が出てしまうので、親要素は無視して末端の各テストの結果のみを出すようにする
                if (result.HasChildren) return;

                // 結果をログに出力
                //Debug.Log($"{result.FullName}:{result.TestStatus}");

                // テストがコケた場合は詳細も出す
                if (result.TestStatus == TestStatus.Failed)
                {
                    Debug.Log($"{result.Message}");
                    Debug.Log($"{result.StackTrace}");
                }
            }
        }
    }
}