using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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
    public class AttackEffect : EntityTargetEffect
    {
        [RequiredListLength(1, null)]
        [SerializeField]
        private List<ElementPower> _elementPowers;

        [Range(0, 1)][SerializeField] private float _criticalRate;
        private float _fixedCriticalRate => Mathf.Clamp(_criticalRate, 0, 1);

        public AttackEffect(List<ElementPower> elementPowers, float criticalRate)
        {
            _elementPowers = elementPowers;
            _criticalRate = criticalRate;
        }

        public void MultiplyPower(float multiplier)
        {
            foreach (var elementPower in _elementPowers)
            {
                elementPower.MultiplyPower(multiplier);
            }
        }

        public override Color Color => Colors.Red;
        public override Impact Impact => Impact.Harmful;

        public override async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, Vector2Int position, IMap map)
        {
            if (Random.value < _fixedCriticalRate)
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
            return result;
        }

        public override float EvaluatePrice()
        {
            var result = Formula.EvaluateDamage(_elementPowers) * (1 - _fixedCriticalRate) +
                         Formula.EvaluateDamage(_elementPowers, true) * _fixedCriticalRate;
            return result;
        }

        public override string UpgradePathName => "攻撃";
        public override List<UpgradeData> GetUpgrades()
        {
            var upgrades = new List<UpgradeData>();

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

        public override List<IHasUpgrades> GetChildren()
        {
            return _elementPowers.Cast<IHasUpgrades>().ToList();
        }

        public override string Info()
        {
            var info = $"攻撃\n威力: {string.Join(" ", _elementPowers.Select(e => e.Info()))}";
            if (_fixedCriticalRate > 0)
            {
                info += $"\nクリティカル: {_fixedCriticalRate:P0}";
            }

            return info;
        }
    }
}