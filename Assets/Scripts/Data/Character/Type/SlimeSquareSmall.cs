using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SlimeSquareSmall : ICharacterType
    {
        public SlimeSquareSmallType Type; public string TypeName() => "SlimeSquareSmall";
        public string SubtypeName() => $"{TypeName()}{Type}"; public SlimeSquareSmall(SlimeSquareSmallType type) { Type = type; }
    }
}