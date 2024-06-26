using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Dye : ICharacterType
    {
        public DyeType Type;

        public Dye(DyeType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Dye";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}