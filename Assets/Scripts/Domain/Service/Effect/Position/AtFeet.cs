using System.Collections.Generic;
using System.Linq;
using Domain.Model.Effect;
using Domain.Service;
using Domain.Service.Characters;
using UnityEngine;
using Utilities;

namespace Effect.Position
{
    public class AtFeet : IEffectPosition
    {
        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IEffectMap map)
        {
            yield return position;
        }

        public string Info()
        {
            return "その場";
        }
    }
}