using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Scorpion : ICharacterType
    {
        public ScorpionType Type;

        public Scorpion(ScorpionType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Scorpion";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}