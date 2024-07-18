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
        [Required] public EnemyData Character;

        public SpawnCharacterEffect(EnemyData character)
        {
            Character = character;
        }

        public Color Color => Colors.MediumPurple;

        public Impact Impact => Impact.Neutral;

        public UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map)
        {
            foreach (var position in positions)
            {
                map.SpawnEnemy(Character, position);
            }
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0;
        }

        public string Info()
        {
            return "召喚";
        }
    }
}