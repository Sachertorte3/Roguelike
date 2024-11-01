using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
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

        public override Color Color => Colors.Yellow;

        public override Impact Impact => Impact.Harmful;

        public void MultiplyPower(float multiplier)
        {
            foreach (var elementPower in _elementPowers)
            {
                elementPower.MultiplyPower(multiplier);
            }
        }

        public override UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, Vector2Int position, IMap map)
        {
            var value = Formula.Calc(actor, target, _elementPowers);
            var loseValue = target.LoseHp(value);
            actor.GainHp(Mathf.RoundToInt(loseValue * _fixedRate));
            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            var damage = Formula.Calc(actor, target, _elementPowers);
            var heal = damage * _fixedRate;
            var value = Mathf.Min(1,
                Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, target, _elementPowers)) / target.CurrentMaxHp);

            var lostRatio = (float)(actor.CurrentMaxHp - actor.CurrentHp) / actor.CurrentMaxHp;
            var healRatio = (float)heal / actor.CurrentMaxHp;
            if (lostRatio >= healRatio)
            {
                value += healRatio;
            }

            if (lostRatio > 0.5f)
            {
                value += lostRatio;
            }
            return value;
        }

        public override float EvaluatePrice()
        {
            return Formula.EvaluateDamage(_elementPowers) * (1 + _fixedRate);
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
            return upgrades;
        }
        public override Dictionary<string, IHasUpgrades> GetChildren()
        {
            return _elementPowers.ToDictionary(e => e.UpgradePathName, e => (IHasUpgrades)e);
        }

        public override string Info()
        {
            var info = "HP吸収\n威力: ";
            info += string.Join(" ", _elementPowers.Select(e => e.Info()));
            info += $"\n吸収割合: {_fixedRate:P0}";
            return info;
        }
    }
}