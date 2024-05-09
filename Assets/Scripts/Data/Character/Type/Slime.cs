using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Slime : ICharacterType
    {
        public SlimeType Type; public string TypeName() => "Slime";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Slime(SlimeType type) { Type = type; }
    }
}