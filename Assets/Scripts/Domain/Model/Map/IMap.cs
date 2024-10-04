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

namespace Domain.Model.Map
{
    public interface IMap : IPassableChecker, IEffectMap
    {
        public ICharacter Player { get; }
        public bool IsEventExecuting { get; }
        public IReadOnlyCollection<Vector2Int> VisibleArea { get; }
        public IObservableCollection<ICharacter> Characters { get; }
        public IObservableCollection<IItemEntity> Items { get; }
        public HashSet<Vector2Int> GetPassablePositionsInArea(IEnumerable<Vector2Int> area);
        public HashSet<ICharacter> GetCharactersInArea(IEnumerable<Vector2Int> area);
        public HashSet<IItemEntity> GetItemsInArea(IEnumerable<Vector2Int> area);
        public HashSet<IEntity> GetEntitiesInArea(IEnumerable<Vector2Int> area);
        public HashSet<Vector2Int> GetAllLightPassablePositions();
        public IItem? GetItemFromId(Id<IItem> id);
        public bool IsOverlapped(Vector2Int position, EntityLayer layer);
        public bool IsBlank(Vector2Int position, params EntityLayer[] layers);
        public bool IsBlankAndStandable(Vector2Int position, params EntityLayer[] layers);
        public bool IsPassable(Vector2Int position, IAffiliation actor);
        public bool IsPassableOnMap(Vector2Int position);
        public bool IsReachable(Vector2Int from, Vector2Int to, IAffiliation actor);
        public IEventEntity? GetEventEntityAt(Vector2Int position, EntityLayer layer);
        public void UpdateTurn(int turn);
        public void RemoveWalls(IEnumerable<Vector2Int> positions);
        public void SetGrasses(IEnumerable<Vector2Int> positions, bool isGrass);
        public IItemEntity SpawnItem(IItem item, Vector2Int position);
        public ICharacter SpawnRandomEnemy(Vector2Int position, bool? isShiny = null);
        public ICharacter SpawnEnemy(EnemyData enemy, Vector2Int position, IAffiliation? affiliation = null, bool? isSlept = null, bool? isShiny = null);
        public UniTask<Vector2Int> ShowThrowAnimation(Sprite icon, Vector2Int position, Direction8 direction, params EntityLayer[] canHitLayer);
        public Vector2Int FindBlankPositionFrom(Vector2Int position, Func<Vector2Int, bool> isBlankFunc);
        public void RemoveEventEntity(IEventEntity entity);
    }
}