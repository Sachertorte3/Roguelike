using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Utilities.Table;

namespace Domain.Model.Item
{
    [Serializable]
    public class RarityWeightTable<T> : ICorrectionTable<T> where T : IHasRarity
    {
        [RequiredListLength(1, null)] [SerializeField]
        private List<T> items = new();

        public List<T> Items => items;

        public T GetRandomItem(float progress)
        {
            return items[items.Select(items => items.Rarity.GetWeight(progress)).WeightedIndex()];
        }
    }
}