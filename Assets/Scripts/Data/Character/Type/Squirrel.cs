using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Squirrel : ICharacterType
    {
        public SquirrelType Type; public string Name() => "Squirrel";
        public string TypeName() => $"{Name()}{Type}"; public Squirrel(SquirrelType type) { Type = type; }
    }
}