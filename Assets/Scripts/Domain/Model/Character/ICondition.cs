using Domain.Model.Condition;
using Domain.Model.Effect;
using UnityEngine;
using Utilities;

namespace Domain.Model.Character
{
    public interface ICondition : ISerializable<ConditionMemento>
    {
        public ParticleType ParticleType { get; }
        public bool CanAct { get; }
        public bool CausesConfusion { get; }
        public void Inflict(IHasCondition hasCondition);
        public void Delete(IHasCondition hasCondition);
        public void UpdateTurn(IHasCondition hasCondition);
        public bool ShouldDelete(int receivedDamage, bool enemyVisible);
    }
}