using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Golem : ICharacterType
    {
        public GolemType Type; public string Name() => "Golem";
        public string TypeName() => $"{Name()}{Type}"; public Golem(GolemType type) { Type = type; }
    }
}