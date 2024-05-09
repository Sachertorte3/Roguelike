using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Dwarf : ICharacterType
    {
        public DwarfType Type; public string TypeName() => "Dwarf";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Dwarf(DwarfType type) { Type = type; }
    }
}