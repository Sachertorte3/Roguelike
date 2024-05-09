using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Witch : ICharacterType
    {
        public WitchType Type; public string TypeName() => "Witch";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Witch(WitchType type) { Type = type; }
    }
}