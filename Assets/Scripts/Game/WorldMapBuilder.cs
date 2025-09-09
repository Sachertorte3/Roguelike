#nullable enable
using System.Collections.Generic;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Events;
using Domain.Service.Map;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Game
{
    public class WorldMapBuilder
    {
        private readonly Id<IMap> _id;
        private readonly string _seed;
        private readonly List<StairsMemento> _stairs = new();

        public WorldMapBuilder(Id<IMap> id, string seed)
        {
            _id = id;
            _seed = seed;
        }
        private Vector2Int GetRandomBlankPositionInRoom()
        {
            return Vector2Int.zero;
        }
        public Vector2Int GetRandomStairPosition()
        {
            return GetRandomBlankPositionInRoom();
        }
        public void AddMovementEntity(MovementData data)
        {
            if (data.Id != null && data.DestinationId != null)
                _stairs.Add(Stairs.Build(data.Type, GetRandomStairPosition(), data.Id,
                    data.Destination, data.DestinationId));
            else
                _stairs.Add(Stairs.Build(data.Type, GetRandomStairPosition(),
                    data.Destination));
        }
        public MapMemento Build()
        {
            return new MapMemento(
                _id,
                TilemapBuilder.Build(_seed),
                new List<CharacterMemento>(),
                new List<ItemEntityMemento>(),
                EventEntityManager.Build(
                    _stairs,
                    new List<ChestMemento>(),
                    new List<TrapMemento>(),
                    new List<StatueMemento>(),
                    new List<MoneyMemento>(),
                    Option<BonfireMemento>.None,
                    Option<EntityMemento>.None,
                    Option<EntityMemento>.None),
                FireEntityManager.Build(),
                new List<string>(),
                Option<RoomMemento>.None,
                Option<ShopMemento>.None,
                Vector2Int.zero
            );
        }
    }
}