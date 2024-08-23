using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class DigEffect : IEffect
    {
        public Color Color => Colors.Brown;
        public Impact Impact => Impact.Neutral;

        public UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map)
        {
            map.RemoveWalls(positions);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 1;
        }

        public IEnumerable<UpgradeSkill> GenerateUpgrades()
        {
            return new List<UpgradeSkill>();
        }

        public string Info()
        {
            return $"壁堀り";
        }
    }
}