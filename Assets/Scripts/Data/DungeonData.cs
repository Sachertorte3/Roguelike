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
            [SerializeField] public Table<ItemData> Items;
            [SerializeField] public Table<EnemyData> Enemies;
            [SerializeField] public Table<MaterialData> Materials;
            [SerializeField] public Table<WeaponMold> WeaponMolds;
            [SerializeField] public Table<WeaponPrefix> WeaponPrefixes = new();
        }
        [RequiredListLength(1, null)] public List<SectionData> Sections;
    }
}

