#nullable enable


#if UNITY_EDITOR
#endif

using System;
using UnityEngine;
using Utilities;

namespace Domain.Model
{
    public enum Rarity
    {
        Common,//基準: 普通、ありふれたもの。感覚としてはだいたいこれ
        Uncommon,//基準: 少し変わった特徴がある。感覚としてはたまにこれ
        Rare,//基準: 珍しい特徴、強さがある。感覚としては引いたら嬉しい
        Epic,//基準: めったに見ない。感覚としては狙って出すもの
        Legendary//基準: 文句なしに強い。感覚としては1度出るかどうか
    }
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
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }
}