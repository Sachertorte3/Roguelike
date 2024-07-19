#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Item;
using ObservableCollections;
using UnityEngine;

namespace Domain.Model
{
    public interface IMap : IPassableChecker, IEffectMap
    {
        public IReadOnlyCollection<Vector2Int> VisibleArea { get; }
        public IObservableCollection<ICharacter> Characters { get; }
        public IObservableCollection<IItemEntity> Items { get; }
        public HashSet<ICharacter> GetCharactersInArea(IEnumerable<Vector2Int> area);
        public HashSet<Vector2Int> GetAllLightPassablePositions();
        public bool IsOverlapped(Vector2Int position, EntityLayer layer);
        public bool IsBlank(Vector2Int position, EntityLayer layer);
        public bool IsPassable(Vector2Int position);
        public bool IsMapPassable(Vector2Int position);
        public bool IsReachable(Vector2Int from, Vector2Int to);
        public bool IsTouchableEventEntityAt(Vector2Int position, EntityLayer layer);
        public void UpdateTurn(int turn);
        public void RemoveWalls(IEnumerable<Vector2Int> positions);
        public void Touch(Vector2Int position);
        public IItemEntity SpawnItem(IItem item, Vector2Int position);
        public ICharacter SpawnEnemy(EnemyData enemy, Vector2Int position, IAffiliation? affiliation = null, bool? isSlept = null, bool? isShiny = null);
        public Vector2Int FindBlankPositionFrom(Vector2Int position, Func<Vector2Int, bool> isBlankFunc);
    }
}