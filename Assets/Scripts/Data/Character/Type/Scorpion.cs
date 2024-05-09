using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Scorpion : ICharacterType
    {
        public ScorpionType Type; public string Name() => "Scorpion";
        public string TypeName() => $"{Name()}{Type}"; public Scorpion(ScorpionType type) { Type = type; }
    }
}