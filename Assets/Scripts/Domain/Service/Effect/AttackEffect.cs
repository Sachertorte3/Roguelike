using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Service.Logs;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AttackEffect : IEffect
    {
        [RequiredListLength(1, null), SerializeField] private List<ElementPower> _elementPowers;
        [Range(0, 1), SerializeField] private float _criticalRate;
        [SerializeField] private int _blowAwayDistance;
        [SerializeField] private List<AdditionalConditionData> _additionalConditions = new();

        public AttackEffect(List<ElementPower> elementPowers, float criticalRate, List<AdditionalConditionData> additionalConditions, int blowAwayDistance)
        {
            _elementPowers = elementPowers;
            _criticalRate = criticalRate;
            _additionalConditions = additionalConditions;
            _blowAwayDistance = blowAwayDistance;
        }

        public Color Color => Colors.Red;

        public Impact Impact => Impact.Harmful;

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            if (Random.value < _criticalRate)
            {
                var damage = Formula.Calc(actor, target, _elementPowers, true);
                GameLog.Add($"<color=red>クリティカル！{target.GetName(map.Player)}に{damage}のダメージ</color>");
                target.LoseHp(damage);
            }
            else
            {
                var damage = Formula.Calc(actor, target, _elementPowers);
                GameLog.Add($"{target.GetName(map.Player)}に{damage}のダメージ");
                target.LoseHp(damage);
            }
            foreach (var condition in _additionalConditions)
            {
                if (Random.value < condition.Probability)
                {
                    target.AddCondition(condition.Condition.Value.Condition, condition.Condition.Value.RemovalCondition);
                }
            }
            if (_blowAwayDistance > 0)
            {
                await target.BlowAway(DirectionMethods.NearestDirectionFromVector(target.CurrentPosition - actor.CurrentPosition).Value, _blowAwayDistance, map);
            }
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            var result = Mathf.Min(1, Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, target, _elementPowers)) / target.CurrentMaxHp) * (1 - _criticalRate);
            result += Mathf.Min(1, Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, target, _elementPowers, true)) / target.CurrentMaxHp) * _criticalRate;
            result += _additionalConditions.Sum(condition => condition.Probability * condition.Condition.Value.Evaluate(target));
            result += _blowAwayDistance * 0.1f;
            return result;
        }

        public float EvaluateDamage()
        {
            var result = Formula.EvaluateDamage(_elementPowers) * (1 - _criticalRate) + Formula.EvaluateDamage(_elementPowers, true) * _criticalRate;
            result += _additionalConditions.Sum(condition => condition.Probability * condition.Condition.Value.EvaluateDamage());
            result += new BlowAwayEffect(_blowAwayDistance).EvaluateDamage();
            return result;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            var upgrades = new Dictionary<UpgradePath, UpgradeData>();
            foreach (var elementPower in _elementPowers)
            {
                upgrades.Add(new UpgradePath("威力", elementPower.Element.ToString()), new UpgradeData($"[{elementPower.Element}]威力+3", () => elementPower.Upgrade(3)));
            }
            if (_criticalRate > 0 && _criticalRate < 0.9f)
            {
                upgrades.Add(new UpgradePath("クリティカル率"), new UpgradeData("クリティカル率+10%", () => _criticalRate += 0.1f));
            }
            if (_blowAwayDistance > 0)
            {
                upgrades.Add(new UpgradePath("吹き飛ばし距離"), new UpgradeData("吹き飛ばし距離+1", () => _blowAwayDistance += 1));
            }
            return upgrades;
        }

        public string Info()
        {
            var info = $"攻撃\n威力: {string.Join(" ", _elementPowers.Select(e => e.Info()))}";
            if (_criticalRate > 0)
            {
                info += $"\nクリティカル: {_criticalRate:P0}";
            }
            if (_blowAwayDistance > 0)
            {
                info += $"\n吹き飛ばし: {_blowAwayDistance}";
            }
            if (_additionalConditions.Count > 0)
            {
                info += "\n追加状態付与:";
                foreach (var condition in _additionalConditions)
                {
                    info += $"\n{condition.Info()}";
                }
            }

            return info;
        }
    }
}