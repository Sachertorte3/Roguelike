using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Ghast : ICharacterType
    {
        public GhastType Type;

        public Ghast(GhastType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Ghast";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}