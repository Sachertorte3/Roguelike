using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model
{
    public partial class DungeonData
    {
        [Serializable]
        public class RoomData
        {
            [Range(0, 1)] public float ChestChance = 0.1f;
            [MinValue(0)] public int ItemCount = 2;
            [MinValue(0)] public int WeaponCount = 1;
            [MinValue(0)] public int CharacterCount = 2;
        }
    }
}