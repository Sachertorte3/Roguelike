using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Slug : ICharacterType
    {
        public SlugType Type; public string TypeName() => "Slug";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Slug(SlugType type) { Type = type; }
    }
}