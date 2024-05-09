using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Dwarf : ICharacterType
    {
        public DwarfType Type; public string Name() => "Dwarf";
        public string TypeName() => $"{Name()}{Type}"; public Dwarf(DwarfType type) { Type = type; }
    }
}