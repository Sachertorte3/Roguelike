#nullable enable
using Data;
using Data.Condition;
using Model.Characters.Conditions;
using Model.Effect;
using ObservableCollections;

namespace Model.Characters
{
    public interface IStatusManager : IHasCondition, ITarget, ITargetOfEffect
    {
        public int CurrentHp { get; }
        public bool IsDead { get; }
        public IObservableCollection<Condition> Conditions { get; }
        public void UpdateTurn();
    }
}
