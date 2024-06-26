using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Disc : ICharacterType
    {
        public DiscType Type;

        public Disc(DiscType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Disc";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}