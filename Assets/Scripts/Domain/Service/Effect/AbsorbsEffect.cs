using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AbsorbsEffect : IEffect
    {
        [MinValue(1), SerializeField] private int _power;
        [Range(0, 1), SerializeField] private float _rate;

        public AbsorbsEffect(int power, float rate)
        {
            _power = power;
            _rate = rate;
        }

        public Color Color => Colors.Yellow;

        public Impact Impact => Impact.Harmful;

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            var value = Formula.Calc(actor, _power);
            var loseValue = await target.LoseHp(value);
            await actor.GainHp(Mathf.RoundToInt(loseValue * _rate));
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Mathf.Min(1, Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, _power)) / target.CurrentMaxHp);
        }

        public string Info()
        {
            return $"HP吸収\n威力: {_power}\n吸収割合: {_rate * 100}%";
        }
    }
}