using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AlertEffect : IActorlessEffect
    {
        public Color Color => Colors.Red;

        public Impact Impact => Impact.Neutral;

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
    [Serializable]
    public class RandomEffect : IEffect
    {
        [SerializeReference] public List<IEffect> Effects;
        private int _index = -1;
        private bool _notRandomized = true;

        public Color Color => _index == -1 ? Colors.White : Effects[_index].Color;

        public Impact Impact => Impact.Harmful;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IMap map)
        {
            if (_notRandomized)
            {
                _index = Enumerable.Range(0, Effects.Count).GetAtRandom();
                _notRandomized = false;
            }
            return Effects[_index].Apply(actor, target, map);
        }

        public UniTask Apply(IActorOfEffect actor, IEntity target, IMap map)
        {
            if (_notRandomized)
            {
                _index = Enumerable.Range(0, Effects.Count).GetAtRandom();
                _notRandomized = false;
            }
            return Effects[_index].Apply(actor, target, map);
        }

        public UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map)
        {
            if (_notRandomized)
            {
                _index = Enumerable.Range(0, Effects.Count).GetAtRandom();
                _notRandomized = false;
            }
            var effect = Effects[_index];
            _notRandomized = true;
            return effect.Apply(actor, positions, map);
        }

        private int ImpactValue(Impact impact)
        {
            return impact switch
            {
                Impact.Neutral => 0,
                Impact.Harmful => 1,
                Impact.Beneficial => -1,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Effects.Average(effect => ImpactValue(effect.Impact) * effect.Evaluate(actor, target));
        }
        public float Evaluate(IActorOfEffect actor, IEnumerable<Vector2Int> positions)
        {
            return Effects.Average(effect => ImpactValue(effect.Impact) * effect.Evaluate(actor, positions));
        }

        public float EvaluatePrice()
        {
            return Effects.Average(effect => effect.EvaluatePrice());
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            return new Dictionary<UpgradePath, UpgradeData>();
        }

        public string Info()
        {
            return $"ランダム";
        }
    }
}