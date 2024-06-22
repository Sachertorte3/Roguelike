using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Dwarf : ICharacterType
    {
        public DwarfType Type;

        public Dwarf(DwarfType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Dwarf";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}