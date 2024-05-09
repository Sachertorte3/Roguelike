using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Mummy : ICharacterType
    {
        public MummyType Type; public string TypeName() => "Mummy";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Mummy(MummyType type) { Type = type; }
    }
}