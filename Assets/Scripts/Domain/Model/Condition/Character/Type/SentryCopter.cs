using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record SentryCopter : ICharacterType
    {
        public SentryCopterType Type;

        public SentryCopter(SentryCopterType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "SentryCopter";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}