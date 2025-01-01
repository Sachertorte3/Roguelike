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
        Dictionary<string, MapMemento> Maps,
        StatisticsMemento Statistics,
        Dictionary<string, int> Settings,
        float TurnWaitTime, bool IsRollbacked);
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

        public void SaveLight(int turn)
        {
            Log.Debug("[Save]Start Save Light");
            db.SaveTurn(_saveDataSlot, turn);
            Log.Debug("[Save]End Save Light");
        }

        public void SaveFull(SaveData saveData)
        {
            Log.Debug("[Save]Start Save");
            db.Save(_saveDataSlot, JsonUtility.ToJson(saveData.World), saveData.TurnWaitTime);
            db.SaveTurn(_saveDataSlot, saveData.Statistics.Turn);
            foreach (var map in saveData.Maps)
            {
                db.SaveMap(_saveDataSlot, map.Key, JsonUtility.ToJson(map.Value));
            }
            db.SaveStatistics(_saveDataSlot, JsonUtility.ToJson(saveData.Statistics));
            db.SaveSettings(_saveDataSlot, Settings.GetValues().ToSerializable());

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
                var mapData = db.LoadMap(_saveDataSlot, mapId);
                maps.Add(mapId, JsonUtility.FromJson<MapMemento>(mapData));
            }
            var statisticsData = db.LoadStatistics(_saveDataSlot);
            var statistics = JsonUtility.FromJson<StatisticsMemento>(statisticsData);

            var settings = db.LoadSettings();

            var latestTurn = LoadLatestTurn();
            var isRollbacked = latestTurn != statistics.Turn;

            Log.Debug("[Save]End Load");
            return new SaveData(world, maps, statistics, settings, turnWaitTime, isRollbacked);
        }

        private int LoadLatestTurn()
        {
            if (!db.ExistSave(_saveDataSlot))
            {
                return 0;
            }
            return db.LoadLatestTurn(_saveDataSlot);
        }

        public void ClearSave()
        {
            db.ClearSave();
        }
    }
}