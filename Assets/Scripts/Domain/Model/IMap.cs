#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Item;
using ObservableCollections;
using UnityEngine;
using Utilities;

namespace Domain.Model
{
    public interface IMap : IPassableChecker, IEffectMap
    {
        public bool IsEventExecuting { get; }
        public IReadOnlyCollection<Vector2Int> VisibleArea { get; }
        public IObservableCollection<ICharacter> Characters { get; }
        public IObservableCollection<IItemEntity> Items { get; }
        public HashSet<ICharacter> GetCharactersInArea(IEnumerable<Vector2Int> area);
        public HashSet<IEntity> GetEntitiesInArea(IEnumerable<Vector2Int> area);
        public HashSet<Vector2Int> GetAllLightPassablePositions();
        public bool IsOverlapped(Vector2Int position, EntityLayer layer);
        public bool IsBlank(Vector2Int position, params EntityLayer[] layers);
        public bool IsPassable(Vector2Int position);
        public bool IsPassableOnMap(Vector2Int position);
        public bool IsReachable(Vector2Int from, Vector2Int to);
        public bool IsTouchableEventEntityAt(Vector2Int position, EntityLayer layer);
        public void UpdateTurn(int turn);
        public void RemoveWalls(IEnumerable<Vector2Int> positions);
        public UniTask Touch(Vector2Int position);
        public IItemEntity SpawnItem(IItem item, Vector2Int position);
        public ICharacter SpawnEnemy(EnemyData enemy, Vector2Int position, IAffiliation? affiliation = null, bool? isSlept = null, bool? isShiny = null);
        public UniTask<Vector2Int> ShowThrowAnimation(Sprite icon, Vector2Int position, Direction8 direction, params EntityLayer[] canHitLayer);
        public Vector2Int FindBlankPositionFrom(Vector2Int position, Func<Vector2Int, bool> isBlankFunc);
    }
}