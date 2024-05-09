using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SlimeSmall : ICharacterType
    {
        public SlimeSmallType Type; public string Name() => "SlimeSmall";
        public string TypeName() => $"{Name()}{Type}"; public SlimeSmall(SlimeSmallType type) { Type = type; }
    }
}