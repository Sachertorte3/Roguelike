using Codice.Client.BaseCommands;
using Scripts.Model.Characters;
using Scripts.Model.Map;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Model
{
    public class World: IWorldViewer
    {
        private Tilemap _map;
        private readonly CharacterManager _characterManager;
        public World(Tilemap map, CharacterManager characterManager)
        {
            _map = map;
            _characterManager = characterManager;
        }
        public bool IsPassable(Vector2Int position)
        {
            return _map.IsPassable(position) && !_characterManager.GetAllCharacterPositions().Contains(position);
        }
        public bool IsPassableIgnoreWall(Vector2Int position)
        {
            return !_characterManager.GetAllCharacterPositions().Contains(position);
        }
        public IEnumerable<Character> GetCharactersInArea(HashSet<Vector2Int> area)
        {
            return _characterManager.Characters.Where(character => area.Contains(character.Position.CurrentValue));
        }
    }
    public interface IWorldViewer
    {
        public bool IsPassable(Vector2Int position);
        public bool IsPassableIgnoreWall(Vector2Int position);
        public IEnumerable<Character> GetCharactersInArea(HashSet<Vector2Int> area);
    }
    public static class GameManager
    {
        public static IWorldViewer? World;
    }
}
