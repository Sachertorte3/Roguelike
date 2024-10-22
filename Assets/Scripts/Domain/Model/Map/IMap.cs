#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Item;
using ObservableCollections;
using UnityEngine;
using Utilities;

namespace Domain.Model.Map
{
    public interface IMap : IPassableChecker
    {
        public Location Location { get; }
        public ItemDatabase ItemDatabase { get; }
        public ItemPlaceholders ItemPlaceholders { get; }
        public ICharacter Player { get; }
        public bool IsEventExecuting { get; }
        public IReadOnlyCollection<Vector2Int> VisibleArea { get; }

        public IEnumerable<IEntity> Entities { get; }
        public IObservableCollection<ICharacter> Characters { get; }
        public IObservableCollection<IItemEntity> Items { get; }

        public HashSet<Vector2Int> GetAllPositions();
        public IEnumerable<IMapPosition> GetBlankAndStandablePositionsInArea(IEnumerable<Vector2Int> area,
            params EntityLayer[] layers);
        public IMapPosition At(Vector2Int position);
        public bool IsGrass(Vector2Int position);
        public HashSet<Vector2Int> GetAllLightPassablePositions();
        public IItem? GetItemFromId(Id<IItem> id);
        public bool IsInside(Vector2Int position);
        public bool IsReachable(Vector2Int from, Vector2Int to, IHasBehavior actor);
        public List<IEventEntity> GetEventEntityAt(Vector2Int position, EntityLayer layer);
        public UniTask ExecuteTrapAt(Vector2Int position, ICharacter actor);
        public void UpdateTurn(int turn);
        public void RemoveWalls(IEnumerable<Vector2Int> positions);
        public void SetGrasses(IEnumerable<Vector2Int> positions, bool isGrass);
        public void SetIce(IEnumerable<Vector2Int> positions, bool isIce);
        public void SpawnFire(IEnumerable<Vector2Int> positions);
        public IItemEntity SpawnItem(IItem item, Vector2Int position);
        public ICharacter SpawnRandomEnemy(Vector2Int position, bool? isSlept = null, bool? isShiny = null);
        public ICharacter SpawnEnemy(EnemyData enemy, Vector2Int position, IAffiliation? affiliation = null,
            bool? isSlept = null, bool? isShiny = null);
        public UniTask<Vector2Int> ShowThrowAnimation(Sprite icon, Vector2Int position, Direction8 direction,
            int distance, params EntityLayer[] canHitLayer);
        public void SpawnEffect(IEnumerable<Vector2Int> area, Color color);
        public IItemEntity? TryPickUpAt(Vector2Int position, bool isShopItem);
        public Vector2Int FindBlankPositionFrom(Vector2Int position, Func<Vector2Int, bool> isBlankFunc);
        public void RemoveEventEntity(IEventEntity entity);
        public HashSet<Vector2Int> AllCharacterPositions();
        public HashSet<Vector2Int> AllItemPositions();
    }
}