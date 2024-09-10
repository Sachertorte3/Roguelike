using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model
{
    public partial class DungeonBluePrintData
    {
        [Serializable]
        public class RoomData
        {
            [Range(0, 1)] public float ChestChance = 0.1f;
            [MinValue(0)] public float ItemCount = 1;
            [MinValue(0)] public float WeaponCount = 0.2f;
            [MinValue(0)] public float CharacterCount = 1;
        }
    }
}