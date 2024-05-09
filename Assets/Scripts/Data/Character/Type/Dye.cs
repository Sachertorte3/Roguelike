using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Dye : ICharacterType
    {
        public DyeType Type; public string Name() => "Dye";
        public string TypeName() => $"{Name()}{Type}"; public Dye(DyeType type) { Type = type; }
    }
}