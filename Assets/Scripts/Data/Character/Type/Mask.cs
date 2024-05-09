using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Mask : ICharacterType
    {
        public MaskType Type; public string TypeName() => "Mask";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Mask(MaskType type) { Type = type; }
    }
}