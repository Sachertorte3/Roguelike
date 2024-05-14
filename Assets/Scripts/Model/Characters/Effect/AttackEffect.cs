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
using Model.Characters.Effect;
using System.Collections.Generic;

namespace Data
{
    [Serializable]
    public class AttackEffect : IEffect
    {
        [MinValue(1)] public int Power;

        public AttackEffect(int power)
        {
            Power = power;
        }
        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target)
        {
            await target.LoseHp(Formula.Calc(actor, Power));
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Mathf.Min(1, (float)Formula.Calc(actor, Power) / target.MaxHp);
        }

        public string Info()
        {
            return $"攻撃\n威力: {Power}";
        }
    }
}