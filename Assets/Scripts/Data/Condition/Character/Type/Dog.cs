using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Dog : ICharacterType
    {
        public DogType Type;

        public Dog(DogType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Dog";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}