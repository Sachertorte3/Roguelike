using System;

namespace Data.Character.Type
{
    [Serializable]
    public record ElementalOrb : ICharacterType
    {
        public ElementalOrbType Type;

        public ElementalOrb(ElementalOrbType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "ElementalOrb";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}