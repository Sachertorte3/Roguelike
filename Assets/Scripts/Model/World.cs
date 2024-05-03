using Scripts.Model.Characters;
using Scripts.Model.Map;
using UnityEngine;

namespace Scripts.Model
{
    public class World
    {
        private Tilemap _map;
        private CharacterManager _characterManager;
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
    }
}
