using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Mummy : ICharacterType
    {
        public MummyType Type;

        public Mummy(MummyType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Mummy";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}