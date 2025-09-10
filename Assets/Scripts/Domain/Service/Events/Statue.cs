using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Effect;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;
using Utilities.Stats;

namespace Domain.Service.Events
{
    public class Statue : ISerializable<StatueMemento>, IScheduledEventEntity
    {
        public readonly string Name;
        public EntityBase Entity { get; init; }
        private readonly SpawnActorlessEffectSkill _skill;
        private int _attackToBreak;

        public Statue(StatueMemento memento)
        {
            Name = memento.Name;
            Entity = new EntityBase(memento.Entity);
            _skill = new SpawnActorlessEffectSkill(memento.Skill);
            _attackToBreak = memento.AttackToBreak;
            Event = new ScheduledEvent(
                memento.Cycle,
                async (gameManager, map) => { await Execute(map); }
            );
        }

        public IScheduledEvent Event { get; init; }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public StatueMemento Serialize()
        {
            return new StatueMemento(Name, Entity.Serialize(), _skill.Serialize(), Event.WaitTurnData, _attackToBreak);
        }

        public static StatueMemento Build(StatueData statue, Vector2Int position)
        {
            return new StatueMemento(statue.name, EntityBase.Build(position, EntityLayer.Middle),
                SpawnActorlessEffectSkill.Build(statue.Skill), new ResourceData(new StatData(statue.Cycle), statue.Cycle), statue.AttackToBreak);
        }

        private async UniTask Execute(IMap map)
        {
            GameLog.Add(Entity.IsVisible, $"<color=red>{Name}</color>が起動した");
            await _skill.Use(Name, Entity.CurrentPosition, map);
        }

        public void Attacked()
        {
            _attackToBreak -= 1;
            if (_attackToBreak <= 0)
            {
                GameLog.Add(Entity.IsVisible, $"<color=red>{Name}</color>は壊れた");
                Entity.Destroy($"は壊された");
            }
        }
    }
}