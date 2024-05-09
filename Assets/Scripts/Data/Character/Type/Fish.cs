using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Fish : ICharacterType
    {
        public FishType Type; public string Name() => "Fish";
        public string TypeName() => $"{Name()}{Type}"; public Fish(FishType type) { Type = type; }
    }
}