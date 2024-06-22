using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Bug : ICharacterType
    {
        public BugType Type;

        public Bug(BugType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Bug";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}