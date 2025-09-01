#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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
    public interface IDungeonData
    {
        public string Name { get; }
        public Id<IMap> GetStartMapId();
        public List<MapConnection> GetDestinations(Id<IMap> mapId);
        public DungeonMapData CreateMapData(Id<IMap> mapId);
    }
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Dungeon")]
    public class DungeonBluePrintData : ScriptableObject, IDungeonData
    {
        public string Name => name;
        public MapGraph MapGraph;
        public MasterItemDataBase MasterItemDataBase;
        public ItemCategoryWeight SpawnItem;
        public Table<TrapData> Traps;
        [Required] public RarityWeightTable<WeaponPrefix> WeaponPrefixes = new();
        public Table<EnemyData> Npcs;

        public MapNode? GetMapNode(Id<IMap> mapId)
        {
            return MapGraph.nodes
                .OfType<MapNode>()
                .Where(node => node.Map == mapId)
                .FirstOrDefault();
        }

        public Id<IMap> GetStartMapId()
        {
            return MapGraph.nodes
                .OfType<MapNode>()
                .First(node => node.PrevNodes.Count() == 0).Map;
        }

        public List<MapConnection> GetDestinations(Id<IMap> mapId)
        {
            Debug.Log(mapId);
            var connections = new List<MapConnection>();
            var mapNode = GetMapNode(mapId);
            foreach (var prevMap in mapNode.PrevNodes)
            {
                connections.Add(new MapConnection(MovementEntityType.UpStairs, prevMap.Map));
            }
            foreach (var nextMap in mapNode.NextNodes)
            {
                connections.Add(new MapConnection(MovementEntityType.DownStairs, nextMap.Map));
            }
            foreach (var teleportInMap in mapNode.TeleportInNodes)
            {
                connections.Add(new MapConnection(MovementEntityType.MagicCircle, teleportInMap.Map));
            }
            foreach (var teleportOutMap in mapNode.TeleportOutNodes)
            {
                connections.Add(new MapConnection(MovementEntityType.MagicCircle, teleportOutMap.Map));
            }
            return connections;
        }

        public DungeonMapData CreateMapData(Id<IMap> mapId)
        {
            var maxDepth = MapGraph.nodes
                .OfType<MapNode>()
                .Select(node => node.Depth)
                .Max();
            var mapNode = GetMapNode(mapId);
            var sectionData = mapNode.SectionData;
            var floorData = mapNode.FloorData;
            return new DungeonMapData(
                name,
                mapNode.Depth,
                mapNode.Depth / maxDepth,
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