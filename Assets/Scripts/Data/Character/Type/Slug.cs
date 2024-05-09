using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Slug : ICharacterType
    {
        public SlugType Type; public string Name() => "Slug";
        public string TypeName() => $"{Name()}{Type}"; public Slug(SlugType type) { Type = type; }
    }
}