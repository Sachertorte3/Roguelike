using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Fish : ICharacterType
    {
        public FishType Type;

        public Fish(FishType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Fish";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}