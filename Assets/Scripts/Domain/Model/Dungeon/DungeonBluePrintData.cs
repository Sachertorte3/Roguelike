#nullable enable
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using RandomDungeonWithBluePrint;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Utilities.Table;

namespace Domain.Model.Dungeon
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Dungeon")]
    public class DungeonBluePrintData : ScriptableObject, IDungeonData
    {
        public string Name => name;
        [SerializeField] private MapGraph MapGraph;
        public Table<FieldBluePrint> Fields;
        public MasterItemDataBase MasterItemDataBase;
        public ItemCategoryWeight SpawnItem;
        public Table<TrapData> Traps;
        public Table<StatueData> Statues;
        [Required] public RarityWeightTable<WeaponPrefix> WeaponPrefixes;
        public Table<EnemyData> Npcs;

        public Id<IMap> GetStartMapId() => MapGraph.GetStartMapId();
        public List<MapConnection> GetDestinations(Id<IMap> mapId)
        {
            var connections = new List<MapConnection>();
            var mapNode = MapGraph.GetMapNode(mapId);
            foreach (var prevMap in mapNode.PrevMapIds(mapId))
            {
                connections.Add(new MapConnection(MovementEntityType.UpStairs, prevMap));
            }
            foreach (var nextMap in mapNode.NextMapIds(mapId))
            {
                connections.Add(new MapConnection(MovementEntityType.DownStairs, nextMap));
            }
            foreach (var teleportInMap in mapNode.TeleportInMapIds(mapId))
            {
                connections.Add(new MapConnection(MovementEntityType.MagicCircle, teleportInMap));
            }
            foreach (var teleportOutMap in mapNode.TeleportOutMapIds(mapId))
            {
                connections.Add(new MapConnection(MovementEntityType.MagicCircle, teleportOutMap));
            }
            return connections;
        }

        public DungeonMapData CreateMapData(Id<IMap> mapId)
        {
            var maxDepth = MapGraph.GetMaxDepth();
            var mapNode = MapGraph.GetMapNode(mapId);
            var sectionData = mapNode.SectionData;
            var floorData = mapNode.FloorData;
            var enemies = mapNode.Enemies;
            var boss = mapNode.Boss;
            return new DungeonMapData(
                Name: name,
                Depth: mapNode.Depth(mapId),
                Progress: mapNode.Depth(mapId) / maxDepth,
                Type: sectionData.Type,
                Field: floorData.Field ?? Fields.GetRandomItem(),
                ItemDatabase: new ItemDatabase(MasterItemDataBase, SpawnItem),
                WeaponPrefixes: WeaponPrefixes,
                ChestItems: MasterItemDataBase.AllChestItems,
                Traps: Traps,
                Statues: Statues,
                ShopItems: MasterItemDataBase.ShopItems,
                Enemies: enemies,
                Npcs: Npcs,
                ShinyChance: floorData.ShinyChance,
                SleepChance: floorData.SleepChance,
                MimicChance: floorData.MimicChance,
                WeaponChanceInChest: sectionData.WeaponChanceInChest,
                RoundRoomCorner: sectionData.RoundRoomCorner,
                CaveInOneRoom: sectionData.CaveInOneRoom,
                WaterChance: sectionData.WaterChance,
                GrassChance: sectionData.GrassChance,
                ShopChance: floorData.ShopChance,
                MonsterHouseChance: floorData.MonsterHouseChance,
                RestRoomChance: floorData.RestRoomChance,
                LakeChance: floorData.LakeChance,
                ItemAttempt: floorData.ItemCount,
                MoneyAttempt: floorData.MoneyCount,
                MoneyAverage: floorData.MoneyAverage,
                CharacterAttempt: floorData.CharacterCount,
                TrapAttempt: floorData.TrapCount,
                ChestChance: floorData.ChestChance,
                StatueChance: floorData.StatueChance,
                BonfireWeight: floorData.BonfireWeight,
                MagicPotWeight: floorData.MagicPotWeight,
                WorkbenchWeight: floorData.WorkbenchWeight,
                Boss: boss,
                Clerk: sectionData.Clerk,
                Mimic: sectionData.Mimic
            );
        }
    }
}