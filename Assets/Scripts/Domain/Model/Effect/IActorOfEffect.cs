using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IActorOfEffect : IHasAffiliation
    {
        public bool IsShiny { get; }
        public string GetName(IHasAffiliation player);
        public Vector2Int CurrentPosition { get; }
        public Aggression Aggression { get; }
        public UniTask<int> GainHp(int value);
    }
}