using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Klackon : ICharacterType
    {
        public KlackonType Type;

        public Klackon(KlackonType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Klackon";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}