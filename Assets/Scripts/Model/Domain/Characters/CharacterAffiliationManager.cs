using System.Collections.Generic;
using Data;
using Model.Characters;

namespace Model.Domain.Characters
{
    public class CharacterAffiliationManager : IAffiliation
    {
        public CharacterGroup Group { get; private set; }

        public CharacterAffiliationManager(CharacterGroup group)
        {
            Group = group;
        }

        public bool IsAlly(IAffiliation other)
        {
            if (Group == CharacterGroup.Neutral)
                return false;
            if (Group == CharacterGroup.Player)
                return other.Group == CharacterGroup.Player;
            if (Group == CharacterGroup.Enemy)
                return false;

            return false;
        }

        public bool IsEnemy(IAffiliation other)
        {
            if (Group == CharacterGroup.Neutral)
                return false;
            if (Group == CharacterGroup.Player)
                return other.Group == CharacterGroup.Enemy;
            if (Group == CharacterGroup.Enemy)
                return other.Group == CharacterGroup.Player;

            return false;
        }
    }
}