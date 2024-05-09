using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Monolith : ICharacterType
    {
        public MonolithType Type; public string Name() => "Monolith";
        public string TypeName() => $"{Name()}{Type}"; public Monolith(MonolithType type) { Type = type; }
    }
}