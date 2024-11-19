using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Table;

namespace Domain.Model.Dungeon
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Dungeon")]
    public class DungeonBluePrintData : ScriptableObject
    {
        public MasterItemDataBase MasterItemDataBase;
        public ItemCategoryWeight SpawnItem;
        public Table<TrapData> Traps;
        [Required] public RarityWeightTable<WeaponPrefix> WeaponPrefixes = new();
        public Table<EnemyData> Npcs;
        [RequiredListLength(1, null)] public List<SectionData> Sections;

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
                sectionData.Type,
                floorData.Field,
                new ItemDatabase(MasterItemDataBase, SpawnItem),
                WeaponPrefixes,
                MasterItemDataBase.ChestItems,
                Traps,
                MasterItemDataBase.ShopItems,
                sectionData.Enemies,
                Npcs,
                floorData.PrefixChance,
                floorData.ShinyChance,
                floorData.SleepChance,
                floorData.ChestChance,
                floorData.MimicChance,
                sectionData.WeaponChanceInChest,
                sectionData.RoundRoomCorner,
                sectionData.CaveInOneRoom,
                sectionData.WaterChance,
                floorData.GrassChance,
                floorData.ShopChance,
                floorData.MonsterHouseChance,
                floorData.RestRoomChance,
                floorData.ItemCount,
                floorData.MoneyCount,
                floorData.MoneyAverage,
                floorData.CharacterCount,
                floorData.TrapCount,
                floorData.ExistBoss,
                floorData.Boss,
                sectionData.Clerk,
                sectionData.Mimic
            );
        }
    }
}