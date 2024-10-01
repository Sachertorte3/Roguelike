using R3;

namespace Domain.Service.Logs
{
    public static class GameLog
    {
        public static Observable<string> OnLogOutput => _onLogOutput;
        public static Observable<Unit> OnClear => _onClear;
        private static readonly Subject<string> _onLogOutput = new();
        private static readonly Subject<Unit> _onClear = new();
        public static void Add(string log)
        {
            _onLogOutput.OnNext(log);
        }
        public static void Clear()
        {
            _onClear.OnNext(Unit.Default);
        }
    }
}