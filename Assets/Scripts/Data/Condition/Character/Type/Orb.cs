using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Orb : ICharacterType
    {
        public OrbType Type;

        public Orb(OrbType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Orb";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}