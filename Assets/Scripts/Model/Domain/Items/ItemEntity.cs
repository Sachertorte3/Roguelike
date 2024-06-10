#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data.Character;
using Data.Map;
using Data.Setting;
using Model.Domain.Action;
using Model.Domain.Characters;
using Model.Domain.Entities;
using R3;
using UnityEngine;
using Utilities;

namespace Model.Domain.Items
{
    public class ItemEntity : IDisposable, ISerializable<ItemEntityMemento>, IEntity
    {
        private readonly Entity _entity;
        private readonly Subject<OnEffectSpawnedMessage> _onEffectSpawned = new();
        public readonly Item Item;

        public static ItemEntityMemento Build(Vector2Int spawnPosition, Item item)
        {
            return new ItemEntityMemento(
                item.Serialize(),
                new EntityMemento(spawnPosition)
            );
        }
        public ItemEntity(ItemEntityMemento item)
        {
            Item = new Item(item.Item);
            _entity = new Entity(item.Entity);
        }

        public Sprite Icon => Item.Icon;
        public Observable<OnEffectSpawnedMessage> OnEffectSpawned => _onEffectSpawned;
        public Observable<Unit> OnDisabled => Item.RemainingUses.Where(value => value <= 0).AsUnitObservable();
        ~ItemEntity()
        {
            Dispose();
        }
        public void Dispose()
        {
            _entity.Dispose();
            _onEffectSpawned.Dispose();
        }

        public ItemEntityMemento Serialize()
        {
            return new ItemEntityMemento(
                Item.Serialize(),
                _entity.Serialize()
            );
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
                _onEffectSpawned.OnNext(new OnEffectSpawnedMessage(Item.SkillOnThrow.GetArea(CurrentPosition, direction), Item.SkillOnThrow.Color));
                await Item.Use(actor, CurrentPosition, direction, world);
            }
        }
    }
}