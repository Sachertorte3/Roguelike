using System;
using Cysharp.Threading.Tasks;
using Data.Condition;
using Data.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Model.Domain.Effect
{
    [Serializable]
    public class AddConditionEffect : IEffect
    {
        [Required] public RemovalConditionData RemovalCondition;
        [SerializeReference][Required] public IConditionData Condition;

        public AddConditionEffect(IConditionData condition, RemovalConditionData removalCondition)
        {
            Condition = condition;
            RemovalCondition = removalCondition;
        }

        public Impact Impact => Condition.Impact;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, ISpawnPositionGenerator map)
        {
            target.AddCondition(Condition, RemovalCondition);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 1;
        }

        public string Info()
        {
            return $"状態付与: {Condition.Name}";
        }
    }
    [Serializable]
    public class TeleportEffect : IEffect
    {
        public Impact Impact => Impact.Neutral;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, ISpawnPositionGenerator map)
        {
            var position = map.GetAllPassablePositions().GetAtRandom();
            target.Teleport(position);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0;
        }

        public string Info()
        {
            return "テレポート";
        }
    }
}