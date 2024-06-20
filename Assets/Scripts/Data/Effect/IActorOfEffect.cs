using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data.Condition;
using UnityEngine;

namespace Data.Effect
{
    public interface IActorOfEffect
    {
        public Vector2Int CurrentPosition { get; }
        public Aggression Aggression { get; }
        public IAffiliation Affiliation { get; }
        public UniTask<int> GainHp(int value);
    }
}

