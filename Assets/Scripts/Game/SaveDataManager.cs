#nullable enable
using System.Collections.Generic;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Unity.Logging;
using UnityEngine;
using Utilities.Serialize;

namespace Game
{
    public record SaveData(
        WorldMemento World,
        StatisticsMemento Statistics,
        Dictionary<string, MapMemento> Maps,
        float TurnWaitTime);
    public class SaveDataManager
    {
        private SQLiteDatabase db;
        private int _saveDataSlot;

        public SaveDataManager(int id)
        {
            db = new SQLiteDatabase();
            _saveDataSlot = id;
        }

        public void SetSaveDataSlot(int id)
        {
            _saveDataSlot = id;
        }

        public bool IsExistSave()
        {
            return db.ExistSave(_saveDataSlot);
        }

        public void Save(SaveData saveData)
        {
            Log.Debug("[Save]Start Save");
            db.Save(_saveDataSlot, JsonUtility.ToJson(saveData.World), saveData.TurnWaitTime);
            foreach (var map in saveData.Maps)
            {
                db.SaveMap(map.Key, JsonUtility.ToJson(map.Value));
            }
            db.SaveStatistics(_saveDataSlot, JsonUtility.ToJson(saveData.Statistics));
            db.SaveSettings(Settings.GetValues().ToSerializable());

            Log.Debug("[Save]End Save");
        }

        public SaveData? Load()
        {
            Log.Debug("[Save]Start Load");
            if (!db.ExistSave(_saveDataSlot))
            {
                return null;
            }
            var (worldData, turnWaitTime) = db.Load(_saveDataSlot);
            var world = JsonUtility.FromJson<WorldMemento>(worldData);
            Dictionary<string, MapMemento> maps = new();
            foreach (var mapId in world.MapIds)
            {
                var mapData = db.LoadMap(mapId);
                maps.Add(mapId, JsonUtility.FromJson<MapMemento>(mapData));
            }
            var statisticsData = db.LoadStatistics(_saveDataSlot);
            var statistics = JsonUtility.FromJson<StatisticsMemento>(statisticsData);
            var settings = db.LoadSettings();
            Settings.SetValues(settings);

            Log.Debug("[Save]End Load");
            return new SaveData(world, statistics, maps, turnWaitTime);
        }

        public void ClearSave()
        {
            db.ClearSave();
        }
    }
}