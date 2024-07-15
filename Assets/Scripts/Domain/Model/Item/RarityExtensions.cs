#nullable enable

#if UNITY_EDITOR
#endif

using System;
using UnityEngine;
using Utilities;

namespace Domain.Model.Item
{
    public static class RarityExtensions
    {
        public static float GetWeight(this Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => 50,
                Rarity.Uncommon => 30,
                Rarity.Rare => 15,
                Rarity.Epic => 4,
                Rarity.Legendary => 1,
                _ => 0
            };
        }

        public static Color GetColor(this Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => Colors.White,
                Rarity.Uncommon => Colors.Green,
                Rarity.Rare => Colors.SkyBlue,
                Rarity.Epic => Colors.Purple,
                Rarity.Legendary => Colors.Yellow,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}