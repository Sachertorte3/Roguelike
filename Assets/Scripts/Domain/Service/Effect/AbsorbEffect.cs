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
        [MinValue(1)] public int Power;
        [Range(0, 1)] public float Rate;

        public AbsorbsEffect(int power, float rate)
        {
            Power = power;
            Rate = rate;
        }

        public Color Color => Colors.Yellow;

        public Impact Impact => Impact.Harmful;

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            var value = Formula.Calc(actor, Power);
            var loseValue = await target.LoseHp(value);
            await actor.GainHp(Mathf.RoundToInt(loseValue * Rate));
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Mathf.Min(1, Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, Power)) / target.CurrentMaxHp);
        }

        public string Info()
        {
            return $"HP吸収\n威力: {Power}\n吸収割合: {Rate * 100}%";
        }
    }
    [Serializable]
    public class RepairEffect : IEffect
    {
        public Color Color => Colors.Brown;

        public Impact Impact => Impact.Beneficial;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            target.RepairAllItem();
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 1;
        }

        public string Info()
        {
            return $"修復(手持ち全て)";
        }
    }
}