using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Service.Logs;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AddConditionEffect : IActorlessEffect
    {
        [Required][SerializeField] private ScriptableObjectSerializable<ConditionTemplate> _condition;

        [OnInspectorInit("OnProbabilityOfSuccessChanged")]
        [SerializeField]
        [Range(0, 1)]
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
        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, Vector2Int position, IMap map) => Apply(actor.Id, target, map);
        public UniTask Apply(ITargetOfEffect target, Vector2Int position, IMap map) => Apply(Id<IEntity>.Empty, target, map);

        public UniTask Apply(Id<IEntity> actorId, ITargetOfEffect target, IMap map)
        {
            if (Random.value < _probabilityOfSuccess)
            {
                if (Random.value > target.GetConditionResistance(_condition.Value))
                {
                    target.AddCondition(actorId, _condition.Value.Condition, _condition.Value.RemovalCondition);
                }
                else
                {
                    GameLog.Add($"{target.GetName(map.Player)}は{_condition.Value.name}に耐性があるようだ");
                }
            }

            return UniTask.CompletedTask;
        }
        public UniTask Apply(IActorOfEffect actor, IEntity target, Vector2Int position, IMap map) => UniTask.CompletedTask;
        public UniTask Apply(IEntity target, Vector2Int position, IMap map) => UniTask.CompletedTask;

        public UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map) => UniTask.CompletedTask;
        public UniTask Apply(IEnumerable<Vector2Int> positions, IMap map) => UniTask.CompletedTask;

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return _condition.Value.Evaluate(target) * _probabilityOfSuccess;
        }
        public float Evaluate(IActorOfEffect actor, IEnumerable<Vector2Int> positions) => 0;

        public float EvaluatePrice()
        {
            return _condition.Value.EvaluateDamage() * _probabilityOfSuccess;
        }

        public string UpgradePathName => "状態付与";
        public List<UpgradeData> GetUpgrades() => new();
        public Dictionary<string, IHasUpgrades> GetChildren() => new();

        public string Info()
        {
            var info = $"状態付与: {_condition.Value.name}";
            info += $" 成功率: {_probabilityOfSuccess:P0}";

            return info;
        }
    }
}