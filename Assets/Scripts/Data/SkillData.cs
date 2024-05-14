using Cysharp.Threading.Tasks;
using Data.Area;
using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public record SkillData : IHasInfo
    {
        [SerializeReference] public IEffect Effect;
        [SerializeReference] public IArea Area;

        public SkillData(IArea area, IEffect effect)
        {
            Area = area;
            Effect = effect;
        }

        public string Info()
        {
            return $"効果: {Effect.Info()}\n範囲: {Area.Info()}";
        }
    }
    public interface IEffect : IHasInfo
    {
        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target);
        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target);
    }
    public interface IActorOfEffect
    {

    }
    public interface ITargetOfEffect
    {
        public int MaxHp { get; }
        public UniTask LoseHp(int value);
    }
}