using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Frog : ICharacterType
    {
        public FrogType Type;

        public Frog(FrogType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Frog";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}