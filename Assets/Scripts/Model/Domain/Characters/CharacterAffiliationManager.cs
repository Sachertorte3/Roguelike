using System.Collections.Generic;
using Data;
using Data.Character;
using Data.Effect;

namespace Model.Domain.Characters
{
    public class CharacterAffiliationManager : IAffiliation, ISerializable<AffiliationMemento>
    {
        public CharacterAffiliationManager(AffiliationMemento data)
        {
            Group = data.Group;
        }

        public AffiliationMemento Serialize()
        {
            return new AffiliationMemento(Group);
        }

        public CharacterGroup Group { get; private set; }

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