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
    [Serializable]
    public class ShopItemData
    {
        public RarityWeightTable<ItemData> Items;
    }
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Dungeon")]
    public partial class DungeonBluePrintData : ScriptableObject
    {
        public MasterItemDataBase MasterItemDataBase;
        public ItemCategoryWeight SpawnItem;
        public ItemCategoryWeight ChestItem;
        public Table<WeaponMold> WeaponMolds;
        [Required] public RarityWeightTable<WeaponPrefix> WeaponPrefixes = new();
        [RequiredListLength(1, null)] public List<SectionData> Sections;

        [Serializable]
        public class SectionData
        {
            public int Depth => Floors.Sum(floor => floor.Depth);
            [RequiredListLength(1, null)] public List<FloorData> Floors;
            private bool _existChest => Floors.Max(floor => floor.Room.ChestChance) > 0;
            [ShowIf("@_existChest"), Required] public float WeaponChanceInChest;
            private bool _existShop => Floors.Max(floor => floor.ShopChance) > 0;
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
        public bool ExistLevel(int level)
        {
            var Depth = Sections.Sum(section => section.Depth);
            return 0 < level && level <= Depth;
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
                new ItemTable(MasterItemDataBase, SpawnItem),
                sectionData.Materials,
                WeaponMolds,
                WeaponPrefixes,
                new ItemTable(MasterItemDataBase, ChestItem),
                MasterItemDataBase.ShopItems,
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