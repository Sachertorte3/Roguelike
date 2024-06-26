#nullable enable
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Service.Characters.Conditions;
using Domain.Service.Characters.Stats;
using Domain.Service.Effect;
using ObservableCollections;
using R3;

namespace Domain.Service.Characters
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