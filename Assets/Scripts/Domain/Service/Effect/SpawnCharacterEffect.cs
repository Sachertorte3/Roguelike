using System;
using System.Collections.Generic;
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
    public class SpawnCharacterEffect : IEffect
    {
        [Required, SerializeField] private ScriptableObjectSerializable<EnemyData> _character;
        [MinValue(1), SerializeField] private int _count;
        [SerializeField] private bool _inheritsShiny;

        public Color Color => Colors.MediumPurple;

        public Impact Impact => Impact.Neutral;

        public UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map)
        {
            foreach (var position in positions)
            {
                for (var i = 0; i < _count; i++)
                {
                    map.SpawnEnemy(
                        _character.Value,
                        position,
                        actor.Affiliation,
                        isSlept: false,
                        _inheritsShiny ? actor.IsShiny : null
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

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() => new();

        public string Info()
        {
            return $"召喚: {_character.Value.Name}\n{_count}体";
        }
    }
}