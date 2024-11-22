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
        [field: SerializeField] public PlayerMemento Player { get; private set; }
        [field: SerializeField] public bool IsPlayerDead { get; private set; }
        [field: SerializeField] public List<string> MapIds { get; private set; }
        [field: SerializeField] public Location CurrentLocation { get; private set; }
        [field: SerializeField] public ItemPlaceholdersMemento ItemPlaceholders { get; private set; }

        public WorldMemento(Dictionary<string, DungeonMemento> dungeons,
            Dictionary<Location, List<MapConnection>> movements, PlayerMemento player, bool isPlayerDead,
            List<string> mapIds, Location currentLocation, ItemPlaceholdersMemento itemPlaceholders)
        {
            _dungeons = dungeons.ToSerializable();
            _movements = movements.ToSerializable();
            Player = player;
            IsPlayerDead = isPlayerDead;
            MapIds = mapIds;
            CurrentLocation = currentLocation;
            ItemPlaceholders = itemPlaceholders;
        }

        public WorldMemento CopyWith(Dictionary<string, DungeonMemento>? dungeons = null,
            Dictionary<Location, List<MapConnection>>? movements = null, PlayerMemento? player = null,
            bool? isPlayerDead = null, List<string>? mapIds = null, Location? currentLocation = null,
            ItemPlaceholdersMemento? itemPlaceholders = null)
        {
            return new WorldMemento(dungeons ?? Dungeons, movements ?? Movements, player ?? Player, isPlayerDead ?? IsPlayerDead, mapIds ?? MapIds, currentLocation ?? CurrentLocation, itemPlaceholders ?? ItemPlaceholders);
        }

        public WorldMemento RevivePlayer()
        {
            return CopyWith(
                player: Player.CopyWith(
                    character: Player.Character.CopyWith(
                        status: Player.Character.Status.CopyWith(
                            stats: Player.Character.Status.Stats.CopyWith(
                                hp: new ResourceData(Player.Character.Status.Stats.Hp.Max, new Stat(Player.Character.Status.Stats.Hp.Max).CurrentValue)
                            )
                        ),
                        entity: Player.Character.Entity.CopyWith(
                            isDestroyed: false
                        )
                    )
                )
            );
        }
    }
}