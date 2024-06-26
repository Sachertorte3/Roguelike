using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Head : ICharacterType
    {
        public HeadType Type;

        public Head(HeadType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Head";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}