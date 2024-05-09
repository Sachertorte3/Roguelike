using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Orb : ICharacterType
    {
        public OrbType Type; public string TypeName() => "Orb";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Orb(OrbType type) { Type = type; }
    }
}