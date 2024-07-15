#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using ObservableCollections;
using R3;

namespace Domain.Model.Character
{
    public interface IStatusManager : IHasCondition
    {
        public IStats Stats { get; }
        public bool IsDead { get; }
        public Observable<int> OnDamageReceived { get; }
        public Observable<int> OnHealReceived { get; }
        public IObservableCollection<ICondition> Conditions { get; }
        public UniTask UpdateTurn(bool enemyVisible);
    }
}