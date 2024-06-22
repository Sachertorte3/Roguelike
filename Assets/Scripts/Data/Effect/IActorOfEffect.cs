using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data.Condition;
using UnityEngine;

namespace Data.Effect
{
    public interface IActorOfEffect : IHasAffiliation
    {
        public Vector2Int CurrentPosition { get; }
        public Aggression Aggression { get; }
        public UniTask<int> GainHp(int value);
    }

    public interface IHasAffiliation
    {
        public IAffiliation Affiliation { get; }
    }
}