using System.Collections.Generic;
using System.Linq;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Effect.Position
{
    public class NearByCharacter : IEffectPosition
    {
        [MinValue(1)] public int NumberOfTarget = 1;
        public bool TargetAlly;
        public bool TargetEnemy;
        public bool TargetNeutral;
        public bool TargetSelf;

        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction, IEffectMap map)
        {
            var positions = new List<Vector2Int>();
            if (TargetSelf)
                positions.Add(actor.CurrentPosition);
            if (TargetAlly)
                positions.AddRange(map.GetAllyPositions(actor));
            if (TargetNeutral)
                positions.AddRange(map.GetNeutralPositions(actor));
            if (TargetEnemy)
                positions.AddRange(map.GetEnemyPositions(actor));
            return positions.OrderBy(p => Vector2Int.Distance(p, position)).Take(NumberOfTarget);
        }

        public string Info()
        {
            return "近くの敵";
        }
    }
}