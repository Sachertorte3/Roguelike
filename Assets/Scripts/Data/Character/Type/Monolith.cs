using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Monolith : ICharacterType
    {
        public MonolithType Type; public string TypeName() => "Monolith";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Monolith(MonolithType type) { Type = type; }
    }
}