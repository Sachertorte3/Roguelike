using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Mummy : ICharacterType
    {
        public MummyType Type; public string Name() => "Mummy";
        public string TypeName() => $"{Name()}{Type}"; public Mummy(MummyType type) { Type = type; }
    }
}