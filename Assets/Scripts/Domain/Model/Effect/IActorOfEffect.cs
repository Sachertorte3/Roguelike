using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IActorOfEffect : IHasAffiliation
    {
        public Vector2Int CurrentPosition { get; }
        public Aggression Aggression { get; }
        public UniTask<int> GainHp(int value);
    }
}