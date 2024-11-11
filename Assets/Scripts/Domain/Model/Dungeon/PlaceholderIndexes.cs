using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Domain.Model.Dungeon
{
    [Serializable]
    public class PlaceholderIndexes
    {
        [SerializeField] private List<int> _prefixIndexes = new();
        [SerializeField] private List<int> _placeholderIndexes = new();

        public PlaceholderIndexes(CategoryPlaceholders placeholders)
        {
            if (placeholders._placeholderPrefixes.Count == 0)
            {
                for (var i = 0; i < placeholders._placeholders.Count; i++)
                {
                    _prefixIndexes.Add(-1);
                    _placeholderIndexes.Add(i);
                }
            }
            else
            {
                for (var i = 0; i < placeholders._placeholderPrefixes.Count; i++)
                {
                    for (var j = 0; j < placeholders._placeholders.Count; j++)
                    {
                        _prefixIndexes.Add(i);
                        _placeholderIndexes.Add(j);
                    }
                }
            }
        }

        public string GetAtRandomAndRemove(CategoryPlaceholders placeholders)
        {
            var index = Random.Range(0, _placeholderIndexes.Count);

            var prefixIndex = _prefixIndexes[index];
            _prefixIndexes.RemoveAt(index);
            var placeholderIndex = _placeholderIndexes[index];
            _placeholderIndexes.RemoveAt(index);

            var prefix = prefixIndex != -1 ? placeholders._placeholderPrefixes[prefixIndex] : "";
            return $"{prefix}{placeholders._placeholders[placeholderIndex]}{placeholders._placeholderSuffix}";
        }
    }
}