using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Brain : ICharacterType
    {
        public BrainType Type;

        public Brain(BrainType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Brain";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}