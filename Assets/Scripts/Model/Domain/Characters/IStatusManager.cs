#nullable enable
using Data.Condition;
using Data.Effect;
using Model.Domain.Characters.Conditions;
using Model.Domain.Effect;
using ObservableCollections;
using R3;

namespace Model.Domain.Characters
{
    public interface IStatusManager : IHasCondition, ITarget, ITargetOfEffect
    {
        public bool IsDead { get; }
        public Observable<int> OnDamageReceived { get; }
        public Observable<int> OnHealReceived { get; }
        public IObservableCollection<Condition> Conditions { get; }
        public void UpdateTurn();
    }
}