using System;
using Utilities;

namespace Domain.Model
{
    [Serializable]
    public class ItemCategoryWeight
    {
        public float Consumables;
        public float Weapons;
        public float Artifacts;
        public float UpgradeMaterials;
        public ItemCategory GetRandomCategory()
        {
            return new float[] { Consumables, Weapons, Artifacts, UpgradeMaterials }.WeightedIndex() switch
            {
                0 => ItemCategory.Consumables,
                1 => ItemCategory.Weapons,
                2 => ItemCategory.Artifacts,
                3 => ItemCategory.UpgradeMaterials,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}