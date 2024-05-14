using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Hare : ICharacterType
    {
        public HareType Type;

        public Hare(HareType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Hare";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}