using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record ElementalOrb : ICharacterType
    {
        public ElementalOrbType Type; public string TypeName() => "ElementalOrb";
        public string SubtypeName() => $"{TypeName()}{Type}"; public ElementalOrb(ElementalOrbType type) { Type = type; }
    }
}