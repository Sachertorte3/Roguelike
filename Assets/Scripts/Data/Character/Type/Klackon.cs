using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Klackon : ICharacterType
    {
        public KlackonType Type; public string Name() => "Klackon";
        public string TypeName() => $"{Name()}{Type}"; public Klackon(KlackonType type) { Type = type; }
    }
}