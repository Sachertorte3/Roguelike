#nullable enable
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Effect;
using Model.Domain.Characters;
using Model.Domain.Characters.Behavior;
using Model.Domain.Items;
using ObservableCollections;
using UnityEngine;

namespace Model.Domain
{
    public interface IMap : IPassableChecker
    {
        public IObservableCollection<Vector2Int> VisibleArea { get; }
        public IObservableCollection<Character> Characters { get; }
        public IObservableCollection<ItemEntity> Items { get; }
        public HashSet<Character> GetCharactersInArea(IEnumerable<Vector2Int> area);
        public HashSet<Vector2Int> GetAllLightPassablePositions();
        public bool IsPassable(Vector2Int position);
        public bool IsMapPassable(Vector2Int position);
        public bool IsReachable(Vector2Int from, Vector2Int to);
        public bool IsEventEntityAt(Vector2Int position, EntityLayer layer);
        public void Touch(Vector2Int position);
        public ItemEntity SpawnItem(Item item, Vector2Int position);
    }
    public static class MapExtensions
    {
        public static IEnumerable<Character> GetVisibleCharacters(this IMap map, IHasBehavior character)
        {
            return map.GetCharactersInArea(character.Area.VisibleArea);
        }
        public static IEnumerable<Character> GetCharactersCanSeePosition(this IMap map, Vector2Int position)
        {
            return map.Characters.Where(character => character.IsVisible(position));
        }
        public static Character? GetCharacter(this IMap map, int id)
        {
            return map.Characters.FirstOrDefault(character => character.Affiliation.Id == id);
        }
    }
}

