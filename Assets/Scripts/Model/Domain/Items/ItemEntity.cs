#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data.Setting;
using Model.Domain.Action;
using Model.Domain.Entities;
using R3;
using UnityEngine;
using Utilities;

namespace Model.Domain.Items
{
    public class ItemEntity : IDisposable, IEntity
    {
        private readonly Entity _entity;
        private readonly Subject<IEnumerable<Vector2Int>> _onSpawnEffect = new();
        public readonly Item Item;

        public ItemEntity(Vector2Int spawnPosition, Item item)
        {
            Item = item;
            _entity = new Entity(spawnPosition);
        }

        public Sprite Icon => Item.Icon;
        public Observable<IEnumerable<Vector2Int>> OnSpawnEffect => _onSpawnEffect;
        public Observable<Unit> OnDisabled => Item.RemainingUses.Where(value => value <= 0).AsUnitObservable();

        public void Dispose()
        {
            _entity.Dispose();
            _onSpawnEffect.Dispose();
        }

        public Entity Entity => _entity;

        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;

        public void SetVisiblity(bool visiblity)
        {
            _entity.SetVisibility(visiblity);
        }

        ~ItemEntity()
        {
            Dispose();
        }

        public async UniTask Throw(IActor actor, Direction8 direction, IMap world)
        {
            while (world.IsPassable(CurrentPosition + direction.Vector()))
            {
                await _entity.Move(direction, Settings.ThrowMilliseconds.Value);
            }

            if (world.IsMapPassable(CurrentPosition + direction.Vector()))
            {
                await _entity.Move(direction, Settings.ThrowMilliseconds.Value);
            }

            if (Item.EffectsOnThrow)
            {
                _onSpawnEffect.OnNext(Item.Skill.GetArea(CurrentPosition, direction));
                await Item.Use(actor, CurrentPosition, direction, world);
            }
        }
    }
}