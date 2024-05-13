#nullable enable
using Cysharp.Threading.Tasks;
using R3;
using Scripts.Model.Action;
using Scripts.Model.Characters.Effect;
using Scripts.Model.Entities;
using Scripts.Utilities;
using System;
using UnityEngine;

namespace Scripts.Model.Items
{
    public class ItemEntity: IDisposable
    {
        public readonly Item Item;
        private readonly Entity _entity;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => _entity.OnMove;
        public Observable<(Skill skill, Vector2Int position, Direction8 direction)> OnUseSkill => _onUseSkill;
        private readonly Subject<(Skill skill, Vector2Int position, Direction8 direction)> _onUseSkill = new();
        public ItemEntity(Vector2Int spawnPosition, Item item)
        {
            Item = item;
            _entity = new Entity(spawnPosition);
        }
        ~ItemEntity()
        {
            Dispose();
        }
        public void Dispose()
        {
            _entity.Dispose();
            _onUseSkill.Dispose();
        }
        public async UniTask Throw(IActor actor, Direction8 direction)
        {
            while (Globals.World.IsPassable(CurrentPosition + direction.Vector()))
            {
                await _entity.Move(direction);
            }
            _onUseSkill.OnNext((Item.Skill, CurrentPosition, direction));
            await Item.Use(actor, CurrentPosition, direction);
        }
    }
}
