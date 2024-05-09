using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Butterfly : ICharacterType
    {
        public ButterflyType Type; public string Name() => "Butterfly";
        public string TypeName() => $"{Name()}{Type}"; public Butterfly(ButterflyType type) { Type = type; }
    }
}