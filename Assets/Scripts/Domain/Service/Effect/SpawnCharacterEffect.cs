using System;
using Cysharp.Threading.Tasks;
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

        public async UniTask Apply(IActorOfEffect actor, Vector2Int position, IPassableChecker map)
        {
            await map.SpawnEnemy(Character, position);
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