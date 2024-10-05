using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Dungeon
{
    [Serializable]
    public class RoomData
    {
        [Range(0, 1)] public float ChestChance = 0.1f;
        [MinValue(0)] public float ItemCount = 1;
        [MinValue(0)] public float WeaponCount = 0.5f;
        [MinValue(0)] public float CharacterCount = 1;
        [MinValue(0)] public float TrapCount = 0.5f;

        public RoomData(float chestChance, float itemCount, float weaponCount, float characterCount, float trapCount)
        {
            ChestChance = chestChance;
            ItemCount = itemCount;
            WeaponCount = weaponCount;
            CharacterCount = characterCount;
            TrapCount = trapCount;
        }
    }
}