using ObservableCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Logs
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
