using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AbsorbsEffect : EntityTargetEffect
    {
        [SerializeField] private List<ElementPower> _elementPowers;
        [Range(0, 1)][SerializeField] private float _rate;
        private float _fixedRate => Mathf.Clamp(_rate, 0, 1);
        [Range(0, 1)][SerializeField] private float _criticalRate;
        private float _fixedCriticalRate => Mathf.Clamp(_criticalRate, 0, 1);

        public override Color Color => Colors.Yellow;

        public override Impact Impact => Impact.Harmful;

        public AbsorbsEffect(List<ElementPower> elementPowers, float rate, float criticalRate)
        {
            _elementPowers = elementPowers;
            _rate = rate;
            _criticalRate = criticalRate;
        }

        public void MultiplyPower(float multiplier)
        {
            foreach (var elementPower in _elementPowers)
            {
                elementPower.MultiplyPower(multiplier);
            }
        }

        public override UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, Vector2Int position, IMap map)
        {
            if (RandUtils.IsLessThanProbability(_fixedCriticalRate))
            {
                var value = Formula.Calc(actor, target, _elementPowers, true);
                GameLog.Add(target.IsVisible, $"<color=red>クリティカル！{target.GetName(map.Player)}に{value}のダメージ</color>");
                var loseValue = target.LoseHp(value, $"は{actor.GetName(map.Player)}の攻撃で殺された");
                actor.GainHp(Mathf.RoundToInt(loseValue * _fixedRate * 2));
            }
            else
            {
                var value = Formula.Calc(actor, target, _elementPowers);
                GameLog.Add(target.IsVisible, $"{target.GetName(map.Player)}に{value}のダメージ");
                var loseValue = target.LoseHp(value, $"は{actor.GetName(map.Player)}の攻撃で殺された");
                actor.GainHp(Mathf.RoundToInt(loseValue * _fixedRate));
            }
            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            var result = Mathf.Min(1,
                             Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, target, _elementPowers)) /
                             target.CurrentMaxHp) *
                         (1 - _fixedCriticalRate);
            result += Mathf.Min(1,
                Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, target, _elementPowers, true)) /
                target.CurrentMaxHp) * _fixedCriticalRate;

            var normalDamage = Formula.Calc(actor, target, _elementPowers);
            var criticalDamage = Formula.Calc(actor, target, _elementPowers, true);
            var expectedDamage = normalDamage * (1 - _fixedCriticalRate) + criticalDamage * _fixedCriticalRate;
            var heal = expectedDamage * _fixedRate;

            var lostRatio = (float)(actor.CurrentMaxHp - actor.CurrentHp) / actor.CurrentMaxHp;
            var healRatio = heal / actor.CurrentMaxHp;
            if (lostRatio >= healRatio)
            {
                result += healRatio;
            }

            if (lostRatio > 0.5f)
            {
                result += lostRatio;
            }

            return result;
        }

        public override float EvaluatePrice()
        {
            return (Formula.EvaluateDamage(_elementPowers) * (1 - _fixedCriticalRate) +
                         Formula.EvaluateDamage(_elementPowers, true) * _fixedCriticalRate) * (1 + _fixedRate);
        }

        public override string UpgradePathName => "HP吸収";

        public override List<UpgradeData> GetUpgrades()
        {
            var upgrades = new List<UpgradeData>();
            if (_rate < 1f)
            {
                upgrades.Add(
                    new UpgradeData(
                        "吸収割合+10%",
                        () => _rate += 0.1f,
                        () => _rate -= 0.1f
                    )
                );
            }

            if (_criticalRate > 0 && _criticalRate < 1f)
            {
                upgrades.Add(
                    new UpgradeData(
                        "クリティカル率+5%",
                        () => _criticalRate += 0.05f,
                        () => _criticalRate -= 0.05f
                    )
                );
            }

            return upgrades;
        }

        public override Dictionary<string, IHasUpgrades> GetChildren()
        {
            return _elementPowers.ToDictionary(e => e.UpgradePathName, e => (IHasUpgrades)e);
        }

        public override string Info()
        {
            var info = string.Join(" ", _elementPowers.Select(e => e.Info()));
            info += "の攻撃を行う\n";
            if (_fixedCriticalRate > 0)
            {
                info += $"そのとき{_fixedCriticalRate:P0}の確率でクリティカルを発生させる\n";
            }
            info += $"与えたダメージの{_fixedRate:P0}を吸収する\n";
            return info;
        }
    }
}