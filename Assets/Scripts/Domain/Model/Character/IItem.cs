#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Service;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Model.Items
{
    public interface IItem : ISerializable<ItemMemento>, IHasInfo
    {
        public bool EffectsOnThrow => SkillOnThrow != null;
        public bool EffectsOnUse => SkillOnUse != null;
        public Sprite Icon { get; }
        public string Name { get; }
        public int Price { get; }
        public ISkill? SkillOnThrow { get; }
        public ISkill? SkillOnUse { get; }
        public bool IsDisabled  { get; }
        public ReadOnlyReactiveProperty<int> RemainingUses { get; }
        public UniTask Use(IActor actor, Vector2Int position, Direction8 direction, IMap world);
        public void Repair();
        public float Evaluate(IActor actor, Vector2Int position, Direction8 direction, IMap world);
    }
}