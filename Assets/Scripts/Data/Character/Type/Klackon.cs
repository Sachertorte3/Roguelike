using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Klackon : ICharacterType
    {
        public KlackonType Type; public string TypeName() => "Klackon";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Klackon(KlackonType type) { Type = type; }
    }
}