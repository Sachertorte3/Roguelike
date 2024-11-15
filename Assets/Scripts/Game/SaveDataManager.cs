#nullable enable
using System.IO;
using Domain.Model.Memento;
using Unity.Logging;
using UnityEngine;

namespace Game
{
    public class SaveDataManager
    {
        public void Save(World world)
        {
            Log.Debug("[Save]Start Save");
            var saveData = world.Serialize();
            var maps = world.SerializeUpdatedMaps();
            WriteData("Save/save.json", JsonUtility.ToJson(saveData));
            foreach (var map in maps)
            {
                Log.Debug($"[Save]Save map: {map.Id}");
                WriteData($"Save/{map.Id}.json", JsonUtility.ToJson(map));
            }

            Log.Debug("[Save]End Save");
        }

        public WorldMemento? Load()
        {
            Log.Debug("[Save]Start Load");
            WorldMemento? world = null;
            var saveData = ReadData("Save/save.json");
            if (saveData != null)
            {
                world = JsonUtility.FromJson<WorldMemento>(saveData);
            }

            Log.Debug("[Save]End Load");
            return world;
        }

        public MapMemento? LoadMap(string mapId)
        {
            var mapData = ReadData($"Save/{mapId}.json");
            return mapData != null ? JsonUtility.FromJson<MapMemento>(mapData) : null;
        }

        private void WriteData(string path, string saveData)
        {
            /*
            if (saveData.Contains("❰") || saveData.Contains("❱"))
            {
                throw new Exception("Save data is corrupted");
            }
            saveData = Regex.Replace(saveData, @"<(.+?)>k__BackingField", "❰$1❱");
            */
            File.WriteAllText(path, saveData);
        }

        private string? ReadData(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var saveDataStr = File.ReadAllText(path);
            /*
            saveDataStr = Regex.Replace(saveDataStr, @"❰(.+?)❱", "<$1>k__BackingField");
            */
            return saveDataStr;
        }

        public void ClearSave()
        {
            var saveDirectory = "Save";
            var jsonFiles = Directory.GetFiles(saveDirectory, "*.json");
            foreach (var file in jsonFiles)
            {
                File.Delete(file);
            }
        }
    }
}