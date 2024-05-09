using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SlimeSquare : ICharacterType
    {
        public SlimeSquareType Type; public string TypeName() => "SlimeSquare";
        public string SubtypeName() => $"{TypeName()}{Type}"; public SlimeSquare(SlimeSquareType type) { Type = type; }
    }
}