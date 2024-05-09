using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SlimeSquareSmall : ICharacterType
    {
        public SlimeSquareSmallType Type; public string Name() => "SlimeSquareSmall";
        public string TypeName() => $"{Name()}{Type}"; public SlimeSquareSmall(SlimeSquareSmallType type) { Type = type; }
    }
}