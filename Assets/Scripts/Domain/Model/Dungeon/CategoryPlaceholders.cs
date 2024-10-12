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
        [SerializeField] private List<string> _placeholderPrefixes;
        [RequiredListLength("@_requiredCount", null), SerializeField] private List<string> _placeholders;
        [SerializeField] private string _placeholderSuffix;
        [HideInInspector, SerializeField] private List<int> _prefixIndexes;
        [HideInInspector, SerializeField] private List<int> _placeholderIndexes;

#if UNITY_EDITOR
        [SerializeField] private string _categoryName;
        private int _count => Directory.GetFiles($"Assets/Database/ItemData/{_categoryName}", "*.asset", SearchOption.AllDirectories).Length;
        private int _requiredCount => Mathf.CeilToInt(_count / Mathf.Max(1f, _placeholderPrefixes.Count));
#endif
        public CategoryPlaceholders(List<string> prefixs, List<string> placeholders, string suffix)
        {
            _placeholderPrefixes = prefixs;
            _placeholders = placeholders;
            _placeholderSuffix = suffix;
        }
        public void InitializeCombinedPlaceholders()
        {
            _prefixIndexes.Clear();
            _placeholderIndexes.Clear();
            if (_placeholderPrefixes.Count == 0)
            {
                for (int i = 0; i < _placeholders.Count; i++)
                {
                    _prefixIndexes.Add(-1);
                    _placeholderIndexes.Add(i);
                }
            }
            else
            {
                for (int i = 0; i < _placeholderPrefixes.Count; i++)
                {
                    for (int j = 0; j < _placeholders.Count; j++)
                    {
                        _prefixIndexes.Add(i);
                        _placeholderIndexes.Add(j);
                    }
                }
            }
        }
        public string GetAtRandomAndRemove()
        {
            var index = Random.Range(0, _placeholderIndexes.Count);

            var prefixIndex = _prefixIndexes[index];
            _prefixIndexes.RemoveAt(index);
            var placeholderIndex = _placeholderIndexes[index];
            _placeholderIndexes.RemoveAt(index);

            var prefix = prefixIndex != -1 ? _placeholderPrefixes[prefixIndex] : "";
            return prefix + _placeholders[placeholderIndex] + _placeholderSuffix;
        }
    }
}