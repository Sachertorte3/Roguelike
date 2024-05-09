using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Mask : ICharacterType
    {
        public MaskType Type; public string Name() => "Mask";
        public string TypeName() => $"{Name()}{Type}"; public Mask(MaskType type) { Type = type; }
    }
}