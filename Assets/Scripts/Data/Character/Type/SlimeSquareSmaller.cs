using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SlimeSquareSmaller : ICharacterType
    {
        public SlimeSquareSmallerType Type; public string Name() => "SlimeSquareSmaller";
        public string TypeName() => $"{Name()}{Type}"; public SlimeSquareSmaller(SlimeSquareSmallerType type) { Type = type; }
    }
}