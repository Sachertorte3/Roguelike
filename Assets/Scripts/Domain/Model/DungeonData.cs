using System;
using System.Collections.Generic;
using Domain.Model.Character;
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
            [Range(0, 1)] public float ShopChance = 0.1f;
            [Range(0, 1)] public float MonsterHouseChance = 0.1f;
            [Required] public FieldBluePrint Field;
            [SerializeField] public RarityWeightTable<ItemData> Items;
            [SerializeField] public Table<EnemyData> Enemies;
            [Required] public EnemyData Clerk;
            [SerializeField] public Table<MaterialData> Materials;
            [SerializeField] public Table<WeaponMold> WeaponMolds;
            [SerializeField] public RarityWeightTable<WeaponPrefix> WeaponPrefixes = new();
        }
    }
}