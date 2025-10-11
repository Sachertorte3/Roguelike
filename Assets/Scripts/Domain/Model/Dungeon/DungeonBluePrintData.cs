#nullable enable
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
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
                name,
                mapNode.Depth(mapId),
                mapNode.Depth(mapId) / maxDepth,
                sectionData.Type,
                floorData.Field,
                new ItemDatabase(MasterItemDataBase, SpawnItem),
                WeaponPrefixes,
                MasterItemDataBase.AllChestItems,
                Traps,
                Statues,
                MasterItemDataBase.ShopItems,
                enemies,
                Npcs,
                floorData.PrefixChance,
                floorData.ShinyChance,
                floorData.SleepChance,
                floorData.MimicChance,
                sectionData.WeaponChanceInChest,
                sectionData.RoundRoomCorner,
                sectionData.CaveInOneRoom,
                sectionData.WaterChance,
                sectionData.GrassChance,
                floorData.ShopChance,
                floorData.MonsterHouseChance,
                floorData.RestRoomChance,
                floorData.LakeChance,
                floorData.ItemCount,
                floorData.MoneyCount,
                floorData.MoneyAverage,
                floorData.CharacterCount,
                floorData.TrapCount,
                floorData.StatueChance,
                floorData.MagicPotChance,
                boss,
                sectionData.Clerk,
                sectionData.Mimic
            );
        }
    }
}