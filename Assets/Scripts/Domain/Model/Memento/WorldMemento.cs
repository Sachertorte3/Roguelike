#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using UnityEngine;
using Utilities.Serialize;
using Utilities.Stats;

namespace Domain.Model.Memento
{
    public class WorldMemento
    {
        [SerializeField] private SerializableDictionary<string, DungeonMemento> _dungeons;

        public Dictionary<string, DungeonMemento> Dungeons =>
            _dungeons.ToDictionary(dungeon => dungeon.Key, dungeon => dungeon.Value);

        [SerializeField] private SerializableDictionary<Location, List<MapConnection>> _movements;
        public Dictionary<Location, List<MapConnection>> Movements => _movements.ToDictionary();
        [field: SerializeField] public CharacterMemento Player { get; private set; }
        [field: SerializeField] public List<string> MapIds { get; private set; }
        [field: SerializeField] public Location CurrentLocation { get; private set; }
        [field: SerializeField] public ItemPlaceholdersMemento ItemPlaceholders { get; private set; }

        public WorldMemento(Dictionary<string, DungeonMemento> dungeons,
            Dictionary<Location, List<MapConnection>> movements, CharacterMemento player,
            List<string> mapIds, Location currentLocation, ItemPlaceholdersMemento itemPlaceholders)
        {
            _dungeons = dungeons.ToSerializable();
            _movements = movements.ToSerializable();
            Player = player;
            MapIds = mapIds;
            CurrentLocation = currentLocation;
            ItemPlaceholders = itemPlaceholders;
        }

        public WorldMemento CopyWith(Dictionary<string, DungeonMemento>? dungeons = null,
            Dictionary<Location, List<MapConnection>>? movements = null, CharacterMemento? player = null,
            List<string>? mapIds = null, Location? currentLocation = null,
            ItemPlaceholdersMemento? itemPlaceholders = null)
        {
            return new WorldMemento(dungeons ?? Dungeons, movements ?? Movements, player ?? Player, mapIds ?? MapIds, currentLocation ?? CurrentLocation, itemPlaceholders ?? ItemPlaceholders);
        }

        public WorldMemento RevivePlayer()
        {
            return CopyWith(player: Player.CopyWith(status: Player.Status.CopyWith(stats: Player.Status.Stats.CopyWith(hp: new ResourceData(Player.Status.Stats.Hp.Max, new Stat(Player.Status.Stats.Hp.Max).CurrentValue)))));
        }
    }
}