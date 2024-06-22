using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Reaper : ICharacterType
    {
        public ReaperType Type;

        public Reaper(ReaperType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Reaper";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}