using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Domain.Model.Item
{
    [Serializable]
    public class RarityWeightTable<T> : ITable<T> where T : IHasRarity
    {
        [RequiredListLength(1, null)]
        [SerializeField]
        private List<T> items = new();

        public T GetRandomItem()
        {
            return items[items.Select(items => items.Rarity.GetWeight()).WeightedIndex()];
        }
    }
}