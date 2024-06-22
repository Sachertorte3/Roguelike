using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Count : ICharacterType
    {
        public CountType Type;

        public Count(CountType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Count";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}