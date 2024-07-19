using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Utilities.Algorithms;

namespace Domain.Service.Effect
{
    [Serializable]
    public class SpawnCharacterEffect : IEffect
    {
        [Required] public EnemyData Character;
        [MinValue(1)] public int Count;
        public bool InheritsShiny;

        public SpawnCharacterEffect(EnemyData character, int count, bool inheritsShiny)
        {
            Character = character;
            Count = count;
            InheritsShiny = inheritsShiny;
        }

        public Color Color => Colors.MediumPurple;

        public Impact Impact => Impact.Neutral;

        public UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map)
        {
            foreach (var position in positions)
            {
                for (var i = 0; i < Count; i++)
                {
                    map.SpawnEnemy(
                        Character,
                        position,
                        actor.Affiliation,
                        false,
                        InheritsShiny ? actor.IsShiny : null
                    );
                }
            }
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0;
        }

        public string Info()
        {
            return $"召喚: {Character.Name}\n{Count}体";
        }
    }
}