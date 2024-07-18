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
        public bool InheritsShiney;

        public SpawnCharacterEffect(EnemyData character, int count, bool inheritsShiney)
        {
            Character = character;
            Count = count;
            InheritsShiney = inheritsShiney;
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
                        BlankFinder.FindBlankPosition(
                            map.IsPassable,
                            map.IsMapPassable,
                            position
                        ),
                        actor.Affiliation,
                        false,
                        InheritsShiney ? actor.IsShiney : null
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