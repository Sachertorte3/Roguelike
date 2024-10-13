using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class SpawnRandomCharacterEffect : IActorlessEffect
    {
        [MinValue(1)] [SerializeField] private int _count;

        public Color Color => Colors.MediumPurple;

        public Impact Impact => Impact.Neutral;

        public UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map)
        {
            return Apply(positions, map);
        }

        public UniTask Apply(IEnumerable<Vector2Int> positions, IMap map)
        {
            var placeablePositions = positions.Where(position => map.CanPlace(position, false, false, false, EntityLayer.Middle));
            if (placeablePositions.Any())
            {
                foreach (var position in placeablePositions.GetAtRandom(_count))
                {
                    map.SpawnRandomEnemy(
                        position,
                        false,
                        false
                    );
                }
            }

            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 50f / CommonSenseParameters.MonsterMaxHealth;
        }

        public float EvaluatePrice()
        {
            return 50f;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            return new Dictionary<UpgradePath, UpgradeData>();
        }

        public string Info()
        {
            return $"召喚: ランダム {_count}体";
        }
    }
}