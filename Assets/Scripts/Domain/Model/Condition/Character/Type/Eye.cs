using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Eye : ICharacterType
    {
        public EyeType Type;

        public Eye(EyeType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Eye";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}