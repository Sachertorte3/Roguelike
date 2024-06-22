using System;

namespace Data.Character.Type
{
    [Serializable]
    public record SkullFlaming : ICharacterType
    {
        public SkullFlamingType Type;

        public SkullFlaming(SkullFlamingType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "SkullFlaming";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}