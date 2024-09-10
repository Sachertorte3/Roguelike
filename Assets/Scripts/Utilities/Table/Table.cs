using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Utilities.Table

{
    [Serializable]
    public class Table<T> : ITable<T>
    {
        [RequiredListLength(1, null)]
        [SerializeField]
        private List<WeightedItem> items = new();

        public T GetRandomItem()
        {
            return items[items.Select(items => items.Weight).WeightedIndex()].Item;
        }

        [Serializable]
        public class WeightedItem
        {
            [HorizontalGroup("Group 1", 0.7f)] public T Item;

            [HorizontalGroup("Group 1", 0.3f)] public float Weight = 1;
        }
    }
}