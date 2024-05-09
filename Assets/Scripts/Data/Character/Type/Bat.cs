using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Bat : ICharacterType
    {
        public BatType Type; public string TypeName() => "Bat";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Bat(BatType type) { Type = type; }
    }
}