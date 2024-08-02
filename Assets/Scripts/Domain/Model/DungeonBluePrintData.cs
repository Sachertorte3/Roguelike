using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Item;
using RandomDungeonWithBluePrint;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Dungeon")]
    public partial class DungeonBluePrintData : ScriptableObject
    {
        public Table<WeaponMold> WeaponMolds;
        [Required] public RarityWeightTable<WeaponPrefix> WeaponPrefixes = new();
        [RequiredListLength(1, null)] public List<SectionData> Sections;

        [Serializable]
        public class SectionData
        {
            public int Depth => Floors.Sum(floor => floor.Depth);
            [RequiredListLength(1, null)] public List<FloorData> Floors;
            public RarityWeightTable<ItemData> Items;
            private bool _existChest => Floors.Max(floor => floor.Room.ChestChance) > 0;
            [ShowIf("@_existChest"), Range(0, 1)] public float WeaponChanceInChest;
            [ShowIf("@_existChest")] public RarityWeightTable<ItemData> ChestItems;
            private bool _existShop => Floors.Max(floor => floor.ShopChance) > 0;
            [ShowIf("@_existShop"), Required] public RarityWeightTable<ItemData> ShopItems;
            [ShowIf("@_existShop"), Required] public EnemyData Clerk;
            public Table<EnemyData> Enemies;
            public Table<MaterialData> Materials;
        }
        [Serializable]
        public class FloorData
        {
            [MinValue(1)] public int Depth;
            [Range(0, 1)] public float PrefixChance = 0.2f;
            [Range(0, 1)] public float ShinyChance = 0.01f;
            [Range(0, 1)] public float SleepChance = 0.5f;
            [Range(0, 1)] public float ShopChance = 0.1f;
            [Range(0, 1)] public float MonsterHouseChance = 0.1f;
            [Required] public FieldBluePrint Field;
            public RoomData Room;
            public bool existBoss;
            [ShowIf("existBoss"), Required] public List<EnemyData> Boss;
        }
        private SectionData GetSectionData(int level)
        {
            var currentDepth = 0;
            foreach (var section in Sections)
            {
                currentDepth += section.Depth;
                if (level <= currentDepth)
                {
                    return section;
                }
            }
            throw new InvalidOperationException("指定されたレベルに対応するセクションが見つかりません。");
        }
        private FloorData GetFloorData(int level)
        {
            var currentDepth = 0;
            foreach (var section in Sections)
            {
                foreach (var floor in section.Floors)
                {
                    currentDepth += floor.Depth;
                    if (level <= currentDepth)
                    {
                        return floor;
                    }
                }
            }
            throw new InvalidOperationException("指定されたレベルに対応するフロアが見つかりません。");
        }
        public DungeonMapData CreateMapData(int level)
        {
            var sectionData = GetSectionData(level);
            var floorData = GetFloorData(level);
            return new DungeonMapData(
                name,
                floorData.Field,
                sectionData.Items,
                sectionData.Materials,
                WeaponMolds,
                WeaponPrefixes,
                sectionData.ChestItems,
                sectionData.ShopItems,
                sectionData.Enemies,
                floorData.PrefixChance,
                floorData.ShinyChance,
                floorData.SleepChance,
                floorData.Room.ChestChance,
                sectionData.WeaponChanceInChest,
                floorData.ShopChance,
                floorData.MonsterHouseChance,
                floorData.Room.ItemCount,
                floorData.Room.WeaponCount,
                floorData.Room.CharacterCount,
                floorData.existBoss,
                floorData.Boss,
                sectionData.Clerk
            );
        }
    }
}