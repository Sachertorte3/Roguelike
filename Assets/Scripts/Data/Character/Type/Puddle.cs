using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Puddle : ICharacterType
    {
        public PuddleType Type; public string TypeName() => "Puddle";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Puddle(PuddleType type) { Type = type; }
    }
}