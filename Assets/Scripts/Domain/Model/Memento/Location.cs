#nullable enable
using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class Location : IEquatable<Location>
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

        public static bool operator ==(Location? a, Location? b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.MapName == b.MapName && a.Level == b.Level;
        }

        public static bool operator !=(Location? a, Location? b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            return obj is Location location && Equals(location);
        }

        public bool Equals(Location other)
        {
            return other.MapName == MapName && other.Level == Level;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MapName, Level);
        }
    }
}