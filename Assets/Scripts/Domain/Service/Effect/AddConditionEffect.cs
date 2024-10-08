using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AddConditionEffect : IActorlessEffect
    {
        [Required] [SerializeField] private ScriptableObjectSerializable<ConditionTemplate> _condition;
        [OnInspectorInit("OnProbabilityOfSuccessChanged")] [SerializeField] [Range(0, 1)]
        private float _probabilityOfSuccess = 1;

        public Color Color => Colors.Purple;

        public Impact Impact => _condition.Value.Condition.Impact;

        public AddConditionEffect(AdditionalConditionData condition)
        {
            _condition = condition.Condition;
            _probabilityOfSuccess = condition.Probability;
        }
#if UNITY_EDITOR
        private void OnProbabilityOfSuccessChanged()
        {
            if (_probabilityOfSuccess == 0)
                _probabilityOfSuccess = 1;
        }
#endif
        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IMap map) => Apply(actor.Id, target);

        public UniTask Apply(ITargetOfEffect target, IMap map) => Apply(Id<IEntity>.Empty, target);

        public UniTask Apply(Id<IEntity> actorId, ITargetOfEffect target)
        {
            if (Random.value < _probabilityOfSuccess)
            {
                target.AddCondition(actorId, _condition.Value.Condition, _condition.Value.RemovalCondition);
            }

            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return _condition.Value.Evaluate(target) * _probabilityOfSuccess;
        }

        public float EvaluatePrice()
        {
            return _condition.Value.EvaluateDamage() * _probabilityOfSuccess;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            return new Dictionary<UpgradePath, UpgradeData>();
        }

        public string Info()
        {
            var info = $"状態付与: {_condition.Value.Condition.Name}";
            info += $" 成功率: {_probabilityOfSuccess:P0}";

            return info;
        }
    }
    [Serializable]
    public class AlertEffect : IActorlessEffect
    {
        public Color Color => Colors.Red;

        public Impact Impact => Impact.Harmful;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IMap map)
        {
            target.ListenToAlert(actor);

            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0.2f;
        }

        public float EvaluatePrice()
        {
            return 20;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            return new Dictionary<UpgradePath, UpgradeData>();
        }

        public string Info()
        {
            return $"警報";
        }
    }
}