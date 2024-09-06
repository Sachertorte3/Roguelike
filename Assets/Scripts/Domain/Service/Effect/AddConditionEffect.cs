using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AddConditionEffect : IEffect
    {
        [Required, SerializeField] private ScriptableObjectSerializable<ConditionTemplate> _condition;

        public AddConditionEffect(IConditionData condition, RemovalConditionData removalCondition)
        {
            _condition = new(new ConditionTemplate(condition, removalCondition));
        }

        public Color Color => Colors.Purple;

        public Impact Impact => _condition.Value.Condition.Impact;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            target.AddCondition(_condition.Value.Condition, _condition.Value.RemovalCondition);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return _condition.Value.Evaluate(target);
        }

        public float EvaluateDamage()
        {
            return _condition.Value.EvaluateDamage();
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() => new();

        public string Info()
        {
            return $"状態付与: {_condition.Value.Condition.Name}";
        }
    }
}