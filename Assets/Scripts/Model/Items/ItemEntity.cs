#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Model.Action;
using Model.Characters.Effect;
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
        private readonly Subject<(Skill skill, Vector2Int position, Direction8 direction)> _onUseSkill = new();
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
        public Observable<(Skill skill, Vector2Int position, Direction8 direction)> OnUseSkill => _onUseSkill;
        public Observable<Unit> OnDisabled => Item.RemainingUses.Where(value => value <= 0).AsUnitObservable();

        public void Dispose()
        {
            _entity.Dispose();
            _onUseSkill.Dispose();
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
            _onUseSkill.OnNext((Item.Skill, CurrentPosition, direction));
            await Item.Use(actor, CurrentPosition, direction);
        }
    }
}