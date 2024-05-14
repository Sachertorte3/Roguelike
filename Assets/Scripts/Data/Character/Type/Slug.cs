using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Slug : ICharacterType
    {
        public SlugType Type;

        public Slug(SlugType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Slug";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}