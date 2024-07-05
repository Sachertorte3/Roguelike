#nullable enable
using System.Collections.Generic;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Items;
using Effect;
using ObservableCollections;
using UnityEngine;

namespace Domain.Service
{
    public interface IMap : IPassableChecker, IEffectMap
    {
        public IObservableCollection<Vector2Int> VisibleArea { get; }
        public IObservableCollection<ICharacter> Characters { get; }
        public IObservableCollection<IItemEntity> Items { get; }
        public HashSet<ICharacter> GetCharactersInArea(IEnumerable<Vector2Int> area);
        public HashSet<Vector2Int> GetAllLightPassablePositions();
        public bool IsPassable(Vector2Int position);
        public bool IsMapPassable(Vector2Int position);
        public bool IsReachable(Vector2Int from, Vector2Int to);
        public bool IsTouchableEventEntityAt(Vector2Int position, EntityLayer layer);
        public void Touch(Vector2Int position);
        public IItemEntity SpawnItem(IItem item, Vector2Int position);
    }
}