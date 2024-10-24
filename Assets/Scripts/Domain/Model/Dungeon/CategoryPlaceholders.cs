using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Domain.Model.Dungeon
{
    [Serializable]
    public class CategoryPlaceholders
    {
        [SerializeField] public List<string> _placeholderPrefixes;
        [RequiredListLength("@_requiredCount", null), SerializeField] public List<string> _placeholders;
        [SerializeField] public string _placeholderSuffix;

#if UNITY_EDITOR
        [SerializeField] private string _categoryName;
        private int _count => Directory.GetFiles($"Assets/Database/ItemData/{_categoryName}", "*.asset", SearchOption.AllDirectories).Length;
        private int _requiredCount => Mathf.CeilToInt(_count / Mathf.Max(1f, _placeholderPrefixes.Count));
#endif
    }
    [Serializable]
    public class PlaceholderIndexes
    {
        [SerializeField] private List<int> _prefixIndexes = new();
        [SerializeField] private List<int> _placeholderIndexes = new();
        public PlaceholderIndexes(CategoryPlaceholders placeholders)
        {
            if (placeholders._placeholderPrefixes.Count == 0)
            {
                for (int i = 0; i < placeholders._placeholders.Count; i++)
                {
                    _prefixIndexes.Add(-1);
                    _placeholderIndexes.Add(i);
                }
            }
            else
            {
                for (int i = 0; i < placeholders._placeholderPrefixes.Count; i++)
                {
                    for (int j = 0; j < placeholders._placeholders.Count; j++)
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