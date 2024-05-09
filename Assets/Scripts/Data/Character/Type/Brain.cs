using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Brain : ICharacterType
    {
        public BrainType Type; public string Name() => "Brain";
        public string TypeName() => $"{Name()}{Type}"; public Brain(BrainType type) { Type = type; }
    }
}