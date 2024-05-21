#nullable enable
using Data;
using Model.Characters;
using Model.Items;
using Model.Map;
using R3;
using RandomDungeonWithBluePrint;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Model
{
    public class MapManager : IMapViewer
    {
        public ITilemapViewer Tilemap => _tilemap;
        private readonly Tilemap _tilemap;
        public CharacterManager CharacterManager { get; init; }
        public ItemManager ItemManager { get; init; }
        public MapManager(FieldBluePrint bluePrint, Character player)
        {
            _tilemap = new(bluePrint);
            CharacterManager = new(player);
            ItemManager = new(player);
        }
        public void Spawn()
        {
            foreach (var position in _tilemap.GetAllPassablePositions().GetAtRandom(10))
                CharacterManager.SpawnCharacter(position);
            var data = Addressables.LoadAssetAsync<DungeonData>("Assets/Database/Dungeon.asset").WaitForCompletion();
            foreach (var position in _tilemap.GetAllPassablePositions().GetAtRandom(30))
                ItemManager.SpawnItem(new Item(data.Items.GetAtRandom()), position);
        }
        public void Load()
        {

        }
        public void UnLoad()
        {

        }
        public bool IsPassable(Vector2Int position)
        {
            return Tilemap.IsPassable(position) && !CharacterManager.GetAllCharacterPositions().Contains(position);
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
        public HashSet<Vector2Int> GetAllPassablePositions()
        {
            var result = _tilemap.GetAllPassablePositions();
            result.ExceptWith(CharacterManager.GetAllCharacterPositions());
            return result;
        }
    }
}