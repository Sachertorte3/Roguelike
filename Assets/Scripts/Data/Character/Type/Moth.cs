using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Moth : ICharacterType
    {
        public MothType Type; public string Name() => "Moth";
        public string TypeName() => $"{Name()}{Type}"; public Moth(MothType type) { Type = type; }
    }
}