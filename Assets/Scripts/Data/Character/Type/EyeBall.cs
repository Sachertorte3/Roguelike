using System;

namespace Data.Character.Type
{
    [Serializable]
    public record EyeBall : ICharacterType
    {
        public EyeBallType Type;

        public EyeBall(EyeBallType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "EyeBall";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}