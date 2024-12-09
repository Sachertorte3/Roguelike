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
        /// <summary>
        /// Get weight of rarity
        /// </summary>
        /// <param name="rarity"></param>
        /// <param name="progress">0~1 </param>
        /// <returns></returns>
        public static float GetWeight(this Rarity rarity, float progress)
        {
            return Mathf.Max(rarity switch
            {
                Rarity.Common => 60,
                Rarity.Uncommon => 30,
                Rarity.Rare => 7,
                Rarity.Epic => 2.5f,
                Rarity.Legendary => 0.5f,
                _ => 0
            } + GetCorrection(rarity, progress), 0);
        }

        private static float GetCorrection(Rarity rarity, float progress)
        {
            return progress * rarity switch
            {
                Rarity.Common => -30,
                Rarity.Uncommon => 0,
                Rarity.Rare => 21,
                Rarity.Epic => 6.5f,
                Rarity.Legendary => 2.5f,
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
                Rarity.Legendary => Colors.Gold,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}