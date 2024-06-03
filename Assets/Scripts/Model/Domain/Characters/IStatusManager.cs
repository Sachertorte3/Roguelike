#nullable enable
using Data.Condition;
using Data.Effect;
using Model.Domain.Characters.Conditions;
using Model.Domain.Effect;
using ObservableCollections;

namespace Model.Domain.Characters
{
    public interface IStatusManager : IHasCondition, ITarget, ITargetOfEffect
    {
        public int CurrentHp { get; }
        public bool IsDead { get; }
        public IObservableCollection<Condition> Conditions { get; }
        public void UpdateTurn();
    }
}