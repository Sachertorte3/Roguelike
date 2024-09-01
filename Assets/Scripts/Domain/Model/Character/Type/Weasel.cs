using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Weasel : ICharacterType
    {
        public WeaselType Type;

        public Weasel(WeaselType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Weasel";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}