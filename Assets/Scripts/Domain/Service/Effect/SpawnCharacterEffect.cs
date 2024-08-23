using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
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

        public SpawnCharacterEffect(EnemyData character, int count, bool inheritsShiny)
        {
            _character = new(character);
            _count = count;
            _inheritsShiny = inheritsShiny;
        }

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
                        false,
                        _inheritsShiny ? actor.IsShiny : null
                    );
                }
            }
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0;
        }

        public IEnumerable<UpgradeSkill> GenerateUpgrades()
        {
            return new List<UpgradeSkill>();
        }

        public string Info()
        {
            return $"召喚: {_character.Value.Name}\n{_count}体";
        }
    }
}