using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Bat : ICharacterType
    {
        public BatType Type; public string Name() => "Bat";
        public string TypeName() => $"{Name()}{Type}"; public Bat(BatType type) { Type = type; }
    }
}