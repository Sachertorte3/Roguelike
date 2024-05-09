using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Puddle : ICharacterType
    {
        public PuddleType Type; public string Name() => "Puddle";
        public string TypeName() => $"{Name()}{Type}"; public Puddle(PuddleType type) { Type = type; }
    }
}