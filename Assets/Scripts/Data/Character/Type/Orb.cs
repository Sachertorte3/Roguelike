using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Orb : ICharacterType
    {
        public OrbType Type; public string Name() => "Orb";
        public string TypeName() => $"{Name()}{Type}"; public Orb(OrbType type) { Type = type; }
    }
}