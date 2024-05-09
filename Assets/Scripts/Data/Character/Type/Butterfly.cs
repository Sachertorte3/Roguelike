using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Butterfly : ICharacterType
    {
        public ButterflyType Type; public string TypeName() => "Butterfly";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Butterfly(ButterflyType type) { Type = type; }
    }
}