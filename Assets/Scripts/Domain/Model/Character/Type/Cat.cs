using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Cat : ICharacterType
    {
        public CatType Type;

        public Cat(CatType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Cat";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}