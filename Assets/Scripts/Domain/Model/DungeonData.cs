using System;
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Item;
using RandomDungeonWithBluePrint;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Dungeon")]
    public class DungeonData : ScriptableObject
    {
        [RequiredListLength(1, null)] public List<SectionData> Sections;

        [Serializable]
        public class SectionData
        {
            [MinValue(1)] public int Depth;
            [Range(0, 1)] public float PrefixChance = 0.2f;
            [Range(0, 1)] public float ShineyChance = 0.01f;
            [Range(0, 1)] public float SleepChance = 0.5f;
            [Range(0, 1)] public float ShopChance = 0.1f;
            [Range(0, 1)] public float MonsterHouseChance = 0.1f;
            [Required] public FieldBluePrint Field;
            public RarityWeightTable<ItemData> Items;
            [ShowIf("@ShopChance > 0")] public RarityWeightTable<ItemData> ShopItems;
            [ShowIf("@ShopChance > 0"), Required] public EnemyData Clerk;
            public Table<EnemyData> Enemies;
            public Table<MaterialData> Materials;
            public Table<WeaponMold> WeaponMolds;
            [ShowIf("@PrefixChance > 0")] public RarityWeightTable<WeaponPrefix> WeaponPrefixes = new();
        }
    }
}