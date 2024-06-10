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
        public Color Color => Colors.Purple;

        public AddConditionEffect(IConditionData condition, RemovalConditionData removalCondition)
        {
            Condition = condition;
            RemovalCondition = removalCondition;
        }

        public Impact Impact => Condition.Impact;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
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
    public class BlowAwayEffect : IEffect
    {
        public Color Color => Colors.LightGreen;

        public BlowAwayEffect()
        {
        }

        public Impact Impact => Impact.Harmful;

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            await target.BlowAway(DirectionMethods.NearestDirectionFromVector(target.CurrentPosition - actor.CurrentPosition).Value, map);
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0;
        }

        public string Info()
        {
            return "吹き飛ばし";
        }
    }
}