using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Mask : ICharacterType
    {
        public MaskType Type;

        public Mask(MaskType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Mask";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}