using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Scorpion : ICharacterType
    {
        public ScorpionType Type; public string TypeName() => "Scorpion";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Scorpion(ScorpionType type) { Type = type; }
    }
}