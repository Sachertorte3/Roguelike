#nullable enable
using System.Collections.Generic;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Unity.Logging;
using UnityEngine;
using Utilities.Serialize;

namespace Game
{
    public record SaveData(WorldMemento World, StatisticsMemento Statistics, Dictionary<string, MapMemento> Maps);
    public class SaveDataManager
    {
        private SQLiteDatabase db;

        public SaveDataManager()
        {
            db = new SQLiteDatabase();
        }

        public void Save(int id, SaveData saveData)
        {
            Log.Debug("[Save]Start Save");
            db.Save(id, JsonUtility.ToJson(saveData.World));
            foreach (var map in saveData.Maps)
            {
                db.SaveMap(map.Key, JsonUtility.ToJson(map.Value));
            }
            db.SaveStatistics(id, JsonUtility.ToJson(saveData.Statistics));
            db.SaveSettings(Settings.GetValues().ToSerializable());

            Log.Debug("[Save]End Save");
        }

        public SaveData? Load(int id)
        {
            Log.Debug("[Save]Start Load");
            if (!db.ExistSave(id))
            {
                return null;
            }
            var saveData = db.Load(id);
            var world = JsonUtility.FromJson<WorldMemento>(saveData);
            Dictionary<string, MapMemento> maps = new();
            foreach (var mapId in world.MapIds)
            {
                var mapData = db.LoadMap(mapId);
                maps.Add(mapId, JsonUtility.FromJson<MapMemento>(mapData));
            }
            var statisticsData = db.LoadStatistics(id);
            var statistics = JsonUtility.FromJson<StatisticsMemento>(statisticsData);
            var settings = db.LoadSettings();
            Settings.SetValues(settings);

            Log.Debug("[Save]End Load");
            return new SaveData(world, statistics, maps);
        }

        public void ClearSave()
        {
            db.ClearSave();
        }
    }
}