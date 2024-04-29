using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
