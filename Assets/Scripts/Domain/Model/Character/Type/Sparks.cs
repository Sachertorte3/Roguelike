using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Sparks : ICharacterType
    {
        public SparksType Type;

        public Sparks(SparksType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Sparks";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}