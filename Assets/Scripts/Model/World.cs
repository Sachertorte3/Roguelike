#nullable enable
using System.Collections.Generic;
using System.Linq;
using Model.Characters;
using Model.Items;
using Model.Map;
using R3;
using UnityEngine;
using VContainer;

namespace Model
{
    public class World : IWorldViewer
    {
        private readonly Tilemap _map;

        [Inject]
        public World(Tilemap map, CharacterManager characterManager, ItemManager itemManager)
        {
            _map = map;
            CharacterManager = characterManager;
            ItemManager = itemManager;

            characterManager.CharacterEvents.OnPositionChanged.Subscribe(move =>
            {
                if (move.Character.Inventory.HasEmptySpace())
                {
                    var item = itemManager.TryPickUp(move.Position);
                    if (item != null) move.Character.TryPickUp(item.Item);
                }
            });

            Globals.World = this;
        }

        public ITilemapViewer Map => _map;
        public CharacterManager CharacterManager { get; init; }
        public ItemManager ItemManager { get; init; }

        public bool IsPassable(Vector2Int position)
        {
            return _map.IsPassable(position) && !CharacterManager.GetAllCharacterPositions().Contains(position);
        }

        public bool IsPassableIgnoreWall(Vector2Int position)
        {
            return !CharacterManager.GetAllCharacterPositions().Contains(position);
        }

        /// <summary>
        ///     Generates and returns a list of characters currently located within the given positions.
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public HashSet<Character> GetCharactersInArea(HashSet<Vector2Int> area)
        {
            return CharacterManager.Characters.Where(character => area.Contains(character.Position.CurrentValue))
                .ToHashSet();
        }
    }
}