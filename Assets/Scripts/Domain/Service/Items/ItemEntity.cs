#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Items;
using Domain.Model.Map;
using Domain.Model.Message;
using Domain.Model.Setting;
using Domain.Service.Action;
using Domain.Service.Characters;
using Domain.Service.Entities;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Items
{
    internal class ItemEntity : IItemEntity
    {
        private readonly Entity _entity;
        private readonly Subject<OnEffectSpawnedMessage> _onEffectSpawned = new();

        public ItemEntity(ItemEntityMemento item)
        {
            Item = new Item(item.Item);
            _entity = new Entity(item.Entity);
        }

        public IItem Item { get; init; }

        public Sprite Icon => Item.Icon;
        public Observable<OnEffectSpawnedMessage> OnEffectSpawned => _onEffectSpawned;
        public Observable<Unit> OnDisabled => Item.RemainingUses.Where(value => value <= 0).AsUnitObservable();

        public void Dispose()
        {
            _entity.Dispose();
            _onEffectSpawned.Dispose();
        }

        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public Observable<Unit> OnDestroyed => _entity.OnDestroyed;

        public void SetVisiblity(bool visiblity)
        {
            _entity.SetVisibility(visiblity);
        }

        public ItemEntityMemento Serialize()
        {
            return new ItemEntityMemento(
                Item.Serialize(),
                _entity.Serialize()
            );
        }

        public async UniTask Throw(IActor actor, Direction8 direction, IMap map)
        {
            while (map.IsPassable(CurrentPosition + direction.Vector()))
            {
                await _entity.Move(direction, Settings.ThrowMilliseconds.Value);
            }

            if (map.IsMapPassable(CurrentPosition + direction.Vector()))
            {
                await _entity.Move(direction, Settings.ThrowMilliseconds.Value);
            }

            if (Item.EffectsOnThrow)
            {
                _onEffectSpawned.OnNext(new OnEffectSpawnedMessage(
                    Item.SkillOnThrow.GetArea(actor, CurrentPosition, direction, map), Item.SkillOnThrow.Color));
                await Item.Use(actor, CurrentPosition, direction, map);
            }
        }

        ~ItemEntity()
        {
            Dispose();
        }
    }
}