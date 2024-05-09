using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SlimeSquare : ICharacterType
    {
        public SlimeSquareType Type; public string Name() => "SlimeSquare";
        public string TypeName() => $"{Name()}{Type}"; public SlimeSquare(SlimeSquareType type) { Type = type; }
    }
}