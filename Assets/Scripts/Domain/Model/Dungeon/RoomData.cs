using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Dungeon
{
    [Serializable]
    public class RoomData
    {
        [Range(0, 1)] public float ChestChance = 0.1f;
        [MinValue(0)] public float ItemCount = 2;
        [MinValue(0)] public float MoneyCount = 1;
        [MinValue(0)] public float MoneyAverage = 100;
        [MinValue(0)] public float CharacterCount = 1;
        [MinValue(0)] public float TrapCount = 0.5f;

        public RoomData(float chestChance, float itemCount, float moneyCount, float moneyAverage, float characterCount, float trapCount)
        {
            ChestChance = chestChance;
            ItemCount = itemCount;
            MoneyCount = moneyCount;
            MoneyAverage = moneyAverage;
            CharacterCount = characterCount;
            TrapCount = trapCount;
        }
    }
}