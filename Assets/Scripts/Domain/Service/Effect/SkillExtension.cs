#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Effect;

namespace Domain.Service.Effect
{
    public static class SkillExtension
    {
        public static TResult Match<TResult>(this ISkill skill, Func<SpawnEffectSkill, TResult> spawnEffectSkillFunc, Func<ItemTargetSkill, TResult> itemTargetSkillFunc)
        {
            return skill switch
            {
                SpawnEffectSkill spawnEffectSkill => spawnEffectSkillFunc(spawnEffectSkill),
                ItemTargetSkill itemTargetSkill => itemTargetSkillFunc(itemTargetSkill),
                _ => throw new ArgumentException("Invalid skill type")
            };
        }
        public static async UniTask<TResult> Match<TResult>(this ISkill skill, Func<SpawnEffectSkill, UniTask<TResult>> spawnEffectSkillFunc, Func<ItemTargetSkill, UniTask<TResult>> itemTargetSkillFunc)
        {
            return skill switch
            {
                SpawnEffectSkill spawnEffectSkill => await spawnEffectSkillFunc(spawnEffectSkill),
                ItemTargetSkill itemTargetSkill => await itemTargetSkillFunc(itemTargetSkill),
                _ => throw new ArgumentException("Invalid skill type")
            };
        }
        public static TResult Match<TResult>(this ISkillMemento memento, Func<SpawnEffectSkillMemento, TResult> spawnEffectSkillFunc, Func<ItemTargetSkillMemento, TResult> itemTargetSkillFunc)
        {
            return memento switch
            {
                SpawnEffectSkillMemento skillMemento => spawnEffectSkillFunc(skillMemento),
                ItemTargetSkillMemento itemTargetSkillMemento => itemTargetSkillFunc(itemTargetSkillMemento),
                _ => throw new ArgumentException("Invalid skill type")
            };
        }
        public static ISkillMemento Serialize(this ISkill skill)
        {
            return skill.Match(
                spawnEffectSkill => (ISkillMemento)spawnEffectSkill.Serialize(),
                itemTargetSkill => (ISkillMemento)itemTargetSkill.Serialize()
            );
        }
        public static ISkill Deserialize(this ISkillMemento memento)
        {
            return memento.Match(
                spawnEffectSkillMemento => (ISkill)new SpawnEffectSkill(spawnEffectSkillMemento),
                itemTargetSkillMemento => (ISkill)new ItemTargetSkill(itemTargetSkillMemento)
            );
        }
    }
}