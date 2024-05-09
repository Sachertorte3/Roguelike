using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Moth : ICharacterType
    {
        public MothType Type; public string TypeName() => "Moth";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Moth(MothType type) { Type = type; }
    }
}