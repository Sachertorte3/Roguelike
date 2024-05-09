using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Squirrel : ICharacterType
    {
        public SquirrelType Type; public string TypeName() => "Squirrel";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Squirrel(SquirrelType type) { Type = type; }
    }
}