using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Fish : ICharacterType
    {
        public FishType Type; public string TypeName() => "Fish";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Fish(FishType type) { Type = type; }
    }
}