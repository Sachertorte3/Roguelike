using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class SpawnRandomCharacterEffect : ActorlessFieldTargetEffect
    {
        [MinValue(1)][SerializeField] private int _count;

        public override Color Color => Colors.MediumPurple;
        public override Impact Impact => Impact.Neutral;

        public override UniTask Apply(IEnumerable<Vector2Int> positions, IMap map)
        {
            var placeablePositions = positions.Where(position => map.At(position).CanPlace(false, false, false, EntityLayer.Middle));
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

        public override float Evaluate(IActorOfEffect actor, IEnumerable<Vector2Int> positions)
        {
            return 50f / CommonSenseParameters.MonsterMaxHealth;
        }

        public override float EvaluatePrice()
        {
            return 50f;
        }

        public override string UpgradePathName => "ランダム召喚";
        public override List<UpgradeData> GetUpgrades() => new();
        public override List<IHasUpgrades> GetChildren() => new();

        public override string Info()
        {
            return $"召喚: ランダム {_count}体";
        }
    }
}