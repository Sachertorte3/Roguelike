using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AbsorbsEffect : IEffect
    {
        [SerializeField] private List<ElementPower> _elementPowers;
        [Range(0, 1)] [SerializeField] private float _rate;

        public AbsorbsEffect(List<ElementPower> elementPowers, float rate)
        {
            _elementPowers = elementPowers;
            _rate = rate;
        }

        public Color Color => Colors.Yellow;

        public Impact Impact => Impact.Harmful;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IMap map)
        {
            var value = Formula.Calc(actor, target, _elementPowers);
            var loseValue = target.LoseHp(value);
            actor.GainHp(Mathf.RoundToInt(loseValue * _rate));
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Mathf.Min(1,
                Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, target, _elementPowers)) / target.CurrentMaxHp);
        }

        public float EvaluatePrice()
        {
            return Formula.EvaluateDamage(_elementPowers) * (1 + _rate);
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            var upgrades = new Dictionary<UpgradePath, UpgradeData>();
            foreach (var elementPower in _elementPowers)
            {
                foreach (var upgrade in elementPower.GetUpgrades())
                {
                    upgrades.Add(upgrade.Key, upgrade.Value);
                }
            }

            if (_rate < 0.9f)
            {
                upgrades.Add(
                    new UpgradePath("吸収割合"),
                    new UpgradeData("吸収割合+10%", () => _rate += 0.1f)
                );
            }

            return upgrades;
        }

        public string Info()
        {
            var info = "HP吸収\n威力: ";
            info += string.Join(" ", _elementPowers.Select(e => e.Info()));
            info += $"\n吸収割合: {_rate * 100}%";
            return info;
        }
    }
}