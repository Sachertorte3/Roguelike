using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Witch : ICharacterType
    {
        public WitchType Type; public string Name() => "Witch";
        public string TypeName() => $"{Name()}{Type}"; public Witch(WitchType type) { Type = type; }
    }
}