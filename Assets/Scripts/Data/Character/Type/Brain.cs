using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Brain : ICharacterType
    {
        public BrainType Type; public string TypeName() => "Brain";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Brain(BrainType type) { Type = type; }
    }
}