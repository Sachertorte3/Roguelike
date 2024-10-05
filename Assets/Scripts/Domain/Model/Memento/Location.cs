using System;
using UnityEngine;

namespace Domain.Model.Map
{
    [Serializable]
    public class Location
    {
        [SerializeField] private string _mapName;
        [SerializeField] private int _level;
        public string MapName => _mapName;
        public int Level => _level;

        public Location(string mapName, int level)
        {
            _mapName = mapName;
            _level = level;
        }
    }
}