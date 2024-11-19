using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Dungeon
{
    [Serializable]
    public class CategoryPlaceholders
    {
        [SerializeField] public List<string> _placeholderPrefixes;

        [RequiredListLength("@_requiredCount", null)] [SerializeField]
        public List<string> _placeholders;

        [SerializeField] public string _placeholderSuffix;

#if UNITY_EDITOR
        [SerializeField] private string _categoryName;

        private int _count => Directory
            .GetFiles($"Assets/Database/ItemData/{_categoryName}", "*.asset", SearchOption.AllDirectories).Length;

        private int _requiredCount => Mathf.CeilToInt(_count / Mathf.Max(1f, _placeholderPrefixes.Count));
#endif
    }
}