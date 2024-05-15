#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Model.Action;
using Model.Effect;
using Model.Entities;
using Model.Setting;
using R3;
using UnityEngine;
using Utilities;

namespace Model.Items
{
    public class ItemEntity : IDisposable
    {
        private readonly Entity _entity;
        private readonly Subject<IEnumerable<Vector2Int>> _onSpawnEffect = new();
        public readonly Item Item;

        public ItemEntity(Vector2Int spawnPosition, Item item)
        {
            Item = item;
            _entity = new Entity(spawnPosition);
        }

        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
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
                await _entity.Move(direction, Settings.ThrowMilliseconds.Value);
            _onSpawnEffect.OnNext(Item.Skill.GetArea(CurrentPosition, direction));
            await Item.Use(actor, CurrentPosition, direction);
        }
    }
}