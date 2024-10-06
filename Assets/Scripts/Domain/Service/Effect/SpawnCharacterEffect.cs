using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class SpawnCharacterEffect : IActorlessEffect
    {
        [Required][SerializeField] private ScriptableObjectSerializable<EnemyData> _character;
        [MinValue(1)][SerializeField] private int _count;
        [SerializeField] private bool _inheritsShiny;

        public Color Color => Colors.MediumPurple;

        public Impact Impact => Impact.Neutral;

        public UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map)
        {
            var placeablePositions = positions.Where(position => map.CanPlace(position, _character.Value.IsFlying));
            if (placeablePositions.Any())
            {
                foreach (var position in placeablePositions.GetAtRandom(_count))
                {
                    map.SpawnEnemy(
                        _character.Value,
                        position,
                        actor.Affiliation,
                        false,
                        _inheritsShiny ? actor.IsShiny : null
                    );
                }
            }

            return UniTask.CompletedTask;
        }

        public UniTask Apply(IEnumerable<Vector2Int> positions, IMap map)
        {
            var placeablePositions = positions.Where(position => map.CanPlace(position, _character.Value.IsFlying));
            if (placeablePositions.Any())
            {
                foreach (var position in placeablePositions.GetAtRandom(_count))
                {
                    map.SpawnEnemy(
                        _character.Value,
                        position,
                        null,
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
            return $"召喚: {_character.Value.Name} {_count}体";
        }
    }
}