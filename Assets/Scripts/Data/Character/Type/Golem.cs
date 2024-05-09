using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Golem : ICharacterType
    {
        public GolemType Type; public string TypeName() => "Golem";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Golem(GolemType type) { Type = type; }
    }
}