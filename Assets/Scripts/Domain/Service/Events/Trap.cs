using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters;
using Domain.Service.Effect;
using Domain.Service.Entities;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Domain.Service.Events
{
    public class Trap : ISerializable<TrapMemento>, IIconEventEntity
    {
        private readonly string _name;
        private readonly Entity _entity;
        private readonly SpawnEffectSkill _skill;
        private readonly float _probabilityOfBreaking;

        public Trap(TrapMemento memento)
        {
            _name = memento.Name;
            _entity = new Entity(memento.Entity);
            _skill = new SpawnEffectSkill(memento.Skill);
            _probabilityOfBreaking = memento.ProbabilityOfBreaking;
            var characterSkill = new CharacterSkill(CharacterSkill.Build(_skill.Serialize(), 0));
            _events = new List<EntityEvent>
            {
                new(
                    "",
                    () => true,
                    async (gameManager, map) =>
                    {
                        await Execute(map, map.Player);
                    }
                )
            };
        }

        public Id<IEntity> Id => _entity.Id;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination, bool isThrown)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public Observable<Unit> OnDestroyed => _entity.OnDestroyed;

        public Sprite Icon => Addressables
            .LoadAssetAsync<Sprite>("MapChip/(Base)BaseChip_pipo.png[(Base)BaseChip_pipo_1260]").WaitForCompletion();

        public string ChoiceMessage => "";
        public bool CanBeCanceled => false;
        private readonly List<EntityEvent> _events;
        public IReadOnlyList<EntityEvent> Events => _events;

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public void Destroy()
        {
            _entity.Destroy();
        }

        public void Dispose()
        {
            _entity.Dispose();
        }

        public void SetVisibility(bool visibility)
        {
            _entity.SetVisibility(visibility);
        }

        public void Teleport(Vector2Int position)
        {
            _entity.Teleport(position);
        }

        public TrapMemento Serialize()
        {
            return new TrapMemento(_name, _entity.Serialize(), _skill.Serialize(), _probabilityOfBreaking);
        }

        public static TrapMemento Build(TrapData trap, Vector2Int position)
        {
            return new TrapMemento(trap.Name, Entity.Build(position, EntityLayer.Bottom),
                SpawnEffectSkill.Build(trap.Skill), trap.ProbabilityOfBreaking);
        }

        public async UniTask Execute(IMap map, IActorOfEffect actor)
        {
            GameLog.Add($"{_name}が起動した");
            await _skill.Use(actor, CurrentPosition, DirectionMethods.AllDirections.GetAtRandom(), map);
            if (Random.value < _probabilityOfBreaking)
            {
                GameLog.Add($"{_name}は壊れた");
                _entity.Destroy();
            }
        }
    }
}