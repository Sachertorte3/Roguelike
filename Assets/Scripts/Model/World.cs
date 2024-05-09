#nullable enable
using R3;
using Scripts.Model.Characters;
using Scripts.Model.Map;
using System;
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
        private readonly CharacterManager _characterManager;
        [Inject]
        public World(Tilemap map, CharacterManager characterManager)
        {
            _map = map;
            _characterManager = characterManager;
            Globals.World = this;
        }
        public bool IsPassable(Vector2Int position)
        {
            return _map.IsPassable(position) && !_characterManager.GetAllCharacterPositions().Contains(position);
        }
        public bool IsPassableIgnoreWall(Vector2Int position)
        {
            return !_characterManager.GetAllCharacterPositions().Contains(position);
        }
        /// <summary>
        /// Generates and returns a list of characters currently located within the given positions.
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public HashSet<Character> GetCharactersInArea(HashSet<Vector2Int> area)
        {
            return _characterManager.Characters.Where(character => area.Contains(character.Position.CurrentValue)).ToHashSet();
        }
    }
}
