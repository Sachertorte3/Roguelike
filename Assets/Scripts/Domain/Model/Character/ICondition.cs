using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Entity;
using Domain.Model.Memento;
using Utilities;

namespace Domain.Model.Character
{
    public interface ICondition : IHasInfo, ISerializable<ConditionMemento>
    {
        public ParticleType ParticleType { get; }
        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor, IPlayer player);
        public void Delete(IHasCondition hasCondition, Id<IEntity> actor, IPlayer player);
        public void UpdateTurn();
        public bool ShouldDelete(bool characterVisible);
        public bool ShouldDeleteByDamage();
        public bool EqualsConditionType(System.Type conditionType);
    }
}