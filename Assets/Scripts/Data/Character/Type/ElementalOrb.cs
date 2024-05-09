using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record ElementalOrb : ICharacterType
    {
        public ElementalOrbType Type; public string Name() => "ElementalOrb";
        public string TypeName() => $"{Name()}{Type}"; public ElementalOrb(ElementalOrbType type) { Type = type; }
    }
}