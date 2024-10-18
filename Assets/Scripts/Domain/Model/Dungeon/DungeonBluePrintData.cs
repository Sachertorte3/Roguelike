using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Effect;
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
        public Placeholders Placeholders;
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

        public DungeonMapData CreateMapData(int level, ItemDatabase itemDatabase)
        {
            var sectionData = GetSectionData(level);
            var floorData = GetFloorData(level);
            return new DungeonMapData(
                Name: name,
                Type: sectionData.Type,
                Field: floorData.Field,
                ItemDatabase: itemDatabase,
                WeaponPrefixes: WeaponPrefixes,
                ChestItems: MasterItemDataBase.ChestItems,
                Traps: Traps,
                MasterItemDataBase.ShopItems,
                Enemies: sectionData.Enemies,
                Npcs: Npcs,
                PrefixChance: floorData.PrefixChance,
                ShinyChance: floorData.ShinyChance,
                SleepChance: floorData.SleepChance,
                ChestChance: floorData.Room.ChestChance,
                MimicChance: floorData.MimicChance,
                WeaponChanceInChest: sectionData.WeaponChanceInChest,
                RoundRoomCorner: sectionData.RoundRoomCorner,
                CaveInOneRoom: sectionData.CaveInOneRoom,
                WaterChance: sectionData.WaterChance,
                GrassChance: floorData.GrassChance,
                ShopChance: floorData.ShopChance,
                MonsterHouseChance: floorData.MonsterHouseChance,
                RestRoomChance: floorData.RestRoomChance,
                ItemAttempt: floorData.Room.ItemCount,
                MoneyAttempt: floorData.Room.MoneyCount,
                MoneyAverage: floorData.Room.MoneyAverage,
                CharacterAttempt: floorData.Room.CharacterCount,
                TrapAttempt: floorData.Room.TrapCount,
                ExistBoss: floorData.ExistBoss,
                Boss: floorData.Boss,
                Clerk: sectionData.Clerk,
                Mimic: sectionData.Mimic
            );
        }
    }
}