using System;
using UnityEngine;
using Utilities;

namespace Domain.Model
{
    public static class RarityExtension
    {
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