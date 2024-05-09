using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Slime : ICharacterType
    {
        public SlimeType Type; public string Name() => "Slime";
        public string TypeName() => $"{Name()}{Type}"; public Slime(SlimeType type) { Type = type; }
    }
}