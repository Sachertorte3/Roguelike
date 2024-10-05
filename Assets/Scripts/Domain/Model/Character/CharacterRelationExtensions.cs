#nullable enable
using System;

namespace Domain.Model.Character
{
    public static class CharacterRelationExtensions
    {
        public static bool MatchesRelation(this CharacterRelation relation, IHasAffiliation character,
            IHasAffiliation other)
        {
            return relation switch
            {
                CharacterRelation.Ally => character.IsAlly(other),
                CharacterRelation.Neutral => character.IsNeutral(other),
                CharacterRelation.Enemy => character.IsEnemy(other),
                _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, null)
            };
        }
    }
}