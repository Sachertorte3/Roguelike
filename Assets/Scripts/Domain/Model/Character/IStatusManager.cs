#nullable enable
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Effect;
using ObservableCollections;
using R3;
using UnityEngine;

namespace Domain.Model.Characters
{
    public interface IStatusManager : IHasCondition
    {
        public IStats Stats { get; }
        public bool IsDead { get; }
        public Observable<int> OnDamageReceived { get; }
        public Observable<int> OnHealReceived { get; }
        public IObservableCollection<ICondition> Conditions { get; }
        public void UpdateTurn(bool enemyVisible);
    }
}