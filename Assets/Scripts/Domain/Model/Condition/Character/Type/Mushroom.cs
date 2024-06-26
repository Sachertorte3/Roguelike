using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Mushroom : ICharacterType
    {
        public MushroomType Type;

        public Mushroom(MushroomType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Mushroom";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}