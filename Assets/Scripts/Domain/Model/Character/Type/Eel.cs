using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Eel : ICharacterType
    {
        public EelType Type;

        public Eel(EelType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Eel";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}