using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Puddle : ICharacterType
    {
        public PuddleType Type;

        public Puddle(PuddleType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Puddle";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}