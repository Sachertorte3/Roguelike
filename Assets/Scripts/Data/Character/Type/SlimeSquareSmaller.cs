using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SlimeSquareSmaller : ICharacterType
    {
        public SlimeSquareSmallerType Type; public string TypeName() => "SlimeSquareSmaller";
        public string SubtypeName() => $"{TypeName()}{Type}"; public SlimeSquareSmaller(SlimeSquareSmallerType type) { Type = type; }
    }
}