#nullable enable
using Domain.Model.Memento;
using Domain.Model.Setting;
using Unity.Logging;
using UnityEngine;
using Utilities.Serialize;

namespace Game
{
    public class SaveDataManager
    {
        private SQLiteDatabase db;
        public SaveDataManager()
        {
            db = new SQLiteDatabase();
        }
        public void Save(World world)
        {
            Log.Debug("[Save]Start Save");
            var saveData = world.Serialize();
            var maps = world.SerializeUpdatedMaps();
            db.Save(JsonUtility.ToJson(saveData));
            foreach (var map in maps)
            {
                Log.Debug($"[Save]Save map: {map.Id}");
                db.SaveMap(map.Id.ToString(), JsonUtility.ToJson(map));
            }
            db.SaveSettings(Settings.GetValues().ToSerializable());

            Log.Debug("[Save]End Save");
        }

        public WorldMemento? Load()
        {
            Log.Debug("[Save]Start Load");
            WorldMemento? world = null;
            var saveData = db.Load();
            if (saveData != null)
            {
                world = JsonUtility.FromJson<WorldMemento>(saveData);
            }
            var settings = db.LoadSettings();
            if (settings != null)
            {
                Settings.SetValues(settings);
            }

            Log.Debug("[Save]End Load");
            return world;
        }

        public MapMemento? LoadMap(string mapId)
        {
            var mapData = db.LoadMap(mapId);
            return mapData != null ? JsonUtility.FromJson<MapMemento>(mapData) : null;
        }

        public void ClearSave()
        {
            db.ClearSave();
        }
    }
}