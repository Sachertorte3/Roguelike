using System;

namespace Data.Character.Type
{
    [Serializable]
    public record BatSmall : ICharacterType
    {
        public BatSmallType Type;

        public BatSmall(BatSmallType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "BatSmall";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}