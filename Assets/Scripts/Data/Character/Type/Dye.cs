using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Dye : ICharacterType
    {
        public DyeType Type; public string TypeName() => "Dye";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Dye(DyeType type) { Type = type; }
    }
}