using ObservableCollections;
using R3;

namespace Domain.Service.Logs
{
    public static class GameLog
    {
        public static Observable<string> OnLogOutput => _onLogOutput;
        private static readonly Subject<string> _onLogOutput = new();
        public static void Add(string log)
        {
            _onLogOutput.OnNext(log);
        }
    }
}