using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SlimeSmall : ICharacterType
    {
        public SlimeSmallType Type; public string TypeName() => "SlimeSmall";
        public string SubtypeName() => $"{TypeName()}{Type}"; public SlimeSmall(SlimeSmallType type) { Type = type; }
    }
}