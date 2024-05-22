#nullable enable
using Cysharp.Threading.Tasks;
using Model.Action;
using Model.Entities;
using Model.Setting;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Model.Items
{
    public class ItemEntity : IDisposable, IEntity
    {
        private readonly Entity _entity;
        public Entity Entity => _entity;
        private readonly Subject<IEnumerable<Vector2Int>> _onSpawnEffect = new();
        public readonly Item Item;
        public Sprite Icon => Item.Icon;

        public ItemEntity(Vector2Int spawnPosition, Item item)
        {
            Item = item;
            _entity = new Entity(spawnPosition);
        }

        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public Observable<IEnumerable<Vector2Int>> OnSpawnEffect => _onSpawnEffect;
        public Observable<Unit> OnDisabled => Item.RemainingUses.Where(value => value <= 0).AsUnitObservable();

        public void Dispose()
        {
            _entity.Dispose();
            _onSpawnEffect.Dispose();
        }

        ~ItemEntity()
        {
            Dispose();
        }

        public void SetVisiblity(bool visiblity)
        {
            _entity.SetVisibility(visiblity);
        }

        public async UniTask Throw(IActor actor, Direction8 direction)
        {
            while (Globals.World.IsPassable(CurrentPosition + direction.Vector()))
            {
                await _entity.Move(direction, Settings.ThrowMilliseconds.Value);
            }
            if (Globals.World.ActiveMap.CurrentValue.Tilemap.IsPassable(CurrentPosition + direction.Vector()))
            {
                await _entity.Move(direction, Settings.ThrowMilliseconds.Value);
            }
            if (Item.EffectsOnThrow)
            {
                _onSpawnEffect.OnNext(Item.Skill.GetArea(CurrentPosition, direction));
                await Item.Use(actor, CurrentPosition, direction);
            }
        }
    }
}