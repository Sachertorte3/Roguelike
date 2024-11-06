using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters;
using Domain.Service.Effect;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Trap : ISerializable<TrapMemento>, IEventEntity
    {
        public readonly string Name;
        public Entity Entity { get; init; }
        private readonly SpawnEffectSkill _skill;
        private readonly float _probabilityOfBreaking;

        public Trap(TrapMemento memento)
        {
            Name = memento.Name;
            Entity = new Entity(memento.Entity);
            _skill = new SpawnEffectSkill(memento.Skill);
            _probabilityOfBreaking = memento.ProbabilityOfBreaking;
            var characterSkill = new CharacterSkill(CharacterSkill.Build(_skill.Serialize(), 0));
            Event = new CharacterEvent(
                (character) => character.StatusManager.IsAffectedByTraps.CurrentValue,
                async (character, gameManager, map) =>
                {
                    await Execute(map, character);
                }
            );
        }

        public IEvent Event { get; init; }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map) => UniTask.CompletedTask;

        public void Dispose() => Entity.Dispose();

        public TrapMemento Serialize()
        {
            return new TrapMemento(Name, Entity.Serialize(), _skill.Serialize(), _probabilityOfBreaking);
        }

        public static TrapMemento Build(TrapData trap, Vector2Int position)
        {
            return new TrapMemento(trap.Name, Entity.Build(position, EntityLayer.Bottom),
                SpawnEffectSkill.Build(trap.Skill), trap.ProbabilityOfBreaking);
        }

        public async UniTask Execute(IMap map, IActorOfEffect actor)
        {
            GameLog.Add($"{Name}が起動した");
            await _skill.Use(actor, Entity.CurrentPosition, DirectionMethods.AllDirections.GetAtRandom(), map);
            if (Random.value < _probabilityOfBreaking)
            {
                GameLog.Add($"{Name}は壊れた");
                Entity.Destroy();
            }
        }
    }
}