using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Data
{
    [Serializable]
    public class Table<T>
    {
        [Serializable]
        public class WeightedItem
        {
            [HorizontalGroup("Group 1", width: 0.7f)]
            public T Item;

            [HorizontalGroup("Group 1", width: 0.3f)]
            public float Weight = 1;
        }
        [RequiredListLength(1, null), SerializeField] private List<WeightedItem> items = new List<WeightedItem>();

        public T GetRandomItem()
        {
            return items[items.Select(items => items.Weight).WeightedIndex()].Item;
        }
    }
}