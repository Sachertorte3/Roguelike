using System;
using Domain.Model.Character;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Table;

namespace Domain.Model.Dungeon
{
    [Serializable]
    public class SectionData
    {
        public MapType Type;
        public bool RoundRoomCorner;
        public bool CaveInOneRoom;
        [Range(0, 1)] public float WaterChance;
        [Range(0, 1), Required] public float WeaponChanceInChest;
        [Required] public EnemyData Mimic;
        [Required] public EnemyData Clerk;
        public Table<EnemyData> Enemies;
    }
}