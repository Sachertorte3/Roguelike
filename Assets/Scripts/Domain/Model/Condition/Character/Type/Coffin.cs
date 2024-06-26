using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Coffin : ICharacterType
    {
        public CoffinType Type;

        public Coffin(CoffinType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Coffin";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}