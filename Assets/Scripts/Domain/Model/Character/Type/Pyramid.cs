using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Pyramid : ICharacterType
    {
        public PyramidType Type;

        public Pyramid(PyramidType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Pyramid";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}