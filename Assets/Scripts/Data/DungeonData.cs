using System;
using System.Collections.Generic;
using Data.Character;
using RandomDungeonWithBluePrint;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Dungeon")]
    public class DungeonData : ScriptableObject
    {
        [Serializable]
        public class SectionData
        {
            public int Depth;
            public float PrefixChance = 0.2f;
            [Required] public FieldBluePrint Field;
            [RequiredListLength(1, null)] public List<ItemData> Items;
            [RequiredListLength(1, null)] public List<EnemyData> Enemies;
            [RequiredListLength(1, null)] public List<MaterialData> Materials;
            [RequiredListLength(1, null)] public List<WeaponMold> WeaponMolds;
            [RequiredListLength(1, null)] public List<WeaponPrefix> WeaponPrefixes;
        }
        [RequiredListLength(1, null)] public List<SectionData> Sections;
    }
}

