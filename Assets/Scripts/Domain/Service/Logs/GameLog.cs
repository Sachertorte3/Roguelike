using ObservableCollections;

namespace Domain.Service.Logs
{
    public static class GameLog
    {
        public static ObservableList<string> Logs = new();

        public static void Add(string log)
        {
            Logs.Add(log);
        }
    }
}