using System;
using Cysharp.Threading.Tasks;
using Data.Area;
using Model.Action;
using Model;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using System.Linq;
using Utilities;
using Model.Effect;
using System.Collections.Generic;
using Data;

namespace Model.Effect
{
    [Serializable]
    public class HealEffect : IEffect
    {
        [MinValue(1)] public int Power;

        public HealEffect(int power)
        {
            Power = power;
        }
        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target)
        {
            await target.GainHp(Formula.Calc(actor, Power));
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Mathf.Min(1, (float)Formula.Calc(actor, Power) / target.MaxHp);
        }

        public string Info()
        {
            return $"回復\n威力: {Power}";
        }
    }
}