#nullable enable
using R3;
using Scripts.Model.Characters;
using Scripts.Model.Items;
using Scripts.Model.Map;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Scripts.Model
{
    public class World : IWorldViewer
    {
        private Tilemap _map;
        public ITilemapViewer Map => _map;
        public CharacterManager CharacterManager { get; init; }
        public ItemManager ItemManager { get; init; }
        [Inject]
        public World(Tilemap map, CharacterManager characterManager, ItemManager itemManager)
        {
            _map = map;
            CharacterManager = characterManager;
            ItemManager = itemManager;

            characterManager.CharacterEvents.OnPositionChanged.Subscribe(move =>
            {
                if (move.Character.HasEmptySpaceInInventory())
                {
                    ItemEntity? item = itemManager.TryPickUp(move.Position);
                    if (item != null)
                    {
                        move.Character.TryPickUp(item.Item);
                    }
                }
            });

            Globals.World = this;
        }
        public bool IsPassable(Vector2Int position)
        {
            return _map.IsPassable(position) && !CharacterManager.GetAllCharacterPositions().Contains(position);
        }
        public bool IsPassableIgnoreWall(Vector2Int position)
        {
            return !CharacterManager.GetAllCharacterPositions().Contains(position);
        }
        /// <summary>
        /// Generates and returns a list of characters currently located within the given positions.
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public HashSet<Character> GetCharactersInArea(HashSet<Vector2Int> area)
        {
            return CharacterManager.Characters.Where(character => area.Contains(character.Position.CurrentValue)).ToHashSet();
        }
    }
}
