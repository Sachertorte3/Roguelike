#nullable enable
using Domain.Model.Condition;
using Domain.Model.Effect;
using Model.Domain.Characters.Conditions;
using Model.Domain.Characters.Stats;
using Model.Domain.Effect;
using ObservableCollections;
using R3;

namespace Model.Domain.Characters
{
    public interface IStatusManager : IHasCondition
    {
        public IStats Stats { get; }
        public bool IsDead { get; }
        public Observable<int> OnDamageReceived { get; }
        public Observable<int> OnHealReceived { get; }
        public IObservableCollection<Condition> Conditions { get; }
        public void UpdateTurn();
    }
}