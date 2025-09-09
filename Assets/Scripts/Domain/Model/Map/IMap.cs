#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using ObservableCollections;
using UnityEngine;
using Utilities;

namespace Domain.Model.Map
{
    public interface IMap : IPassableChecker
    {
        public Id<IMap> Id { get; }
        public ItemDatabase ItemDatabase { get; }
        public ItemPlaceholders ItemPlaceholders { get; }
        public IPlayer Player { get; }
        public bool IsEventExecuting { get; }

        public IObservableCollection<IEntity> Entities { get; }
        public IObservableCollection<ICharacter> Characters { get; }
        public IObservableCollection<IItemEntity> Items { get; }
        public IObservableCollection<IEventEntity> EventEntities { get; }
        public IObservableCollection<IPlayerEventEntity> PlayerEventEntities { get; }
        public IObservableCollection<IScheduledEventEntity> ScheduledEventEntities { get; }

        public HashSet<Vector2Int> GetAllPositions();
        public IEntity? GetEntityFastAt(Vector2Int position, EntityLayer layer);
        public IEnumerable<IEntity> GetEntitiesFastAt(Vector2Int position, IEnumerable<EntityLayer> layers);
        public IEnumerable<IEntity> GetEntitiesFastAt(Vector2Int position);

        public bool IsInside(Vector2Int position);
        public bool IsReachable(Vector2Int from, Vector2Int to, IHasBehavior actor);

        public IItem? GetItemByIdFromWorldOrInventory(Id<IItem> id);

        public UniTask ExecuteTrapAt(Vector2Int position, ICharacter actor);

        public void UpdateTurn(int turn);

        public void RemoveWalls(IEnumerable<Vector2Int> positions);

        public bool IsGrass(Vector2Int position);
        public void SetGrasses(IEnumerable<Vector2Int> positions, bool isGrass);
        public void SetIce(IEnumerable<Vector2Int> positions, bool isIce);

        public void AttackStatue(IEnumerable<Vector2Int> positions);

        public IItemEntity SpawnItem(IItem item, Vector2Int position);
        public ICharacter? SpawnRandomEnemy(Vector2Int position, bool? isSlept = null, bool? isShiny = null);

        public ICharacter SpawnEnemy(EnemyData enemy, Vector2Int position, IAffiliation? affiliation = null,
            bool? isSlept = null, bool? isShiny = null);

        public void SpawnFire(IEnumerable<Vector2Int> positions);

        public UniTask<Vector2Int> ShowThrowAnimation(Sprite icon, Vector2Int position, Direction8 direction,
            int distance, params EntityLayer[] canHitLayer);

        public void SpawnEffect(IEnumerable<Vector2Int> area, Color color);

        public IItemEntity? TryPickUpAt(Vector2Int position, bool canPickUpShopItem);

        public void DropAllItemInStorage(IItem item);

        public Vector2Int FindBlankPositionFrom(Vector2Int position, Func<Vector2Int, bool> isBlankFunc);

        public HashSet<Vector2Int> AllCharacterPositions();
        public HashSet<Vector2Int> AllItemPositions();

        public bool IsVisible(Vector2Int from, Vector2Int to, float radius);
        public HashSet<Vector2Int> GetVisibleArea(Vector2Int from, float radius);
        public HashSet<Vector2Int> GetFullVisibleArea();

        public HashSet<Vector2Int> ComputeCircle(Func<Vector2Int, bool> isTileBlocked, Vector2Int position,
            float radius);
    }
}