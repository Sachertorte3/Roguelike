using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SkullSmall : ICharacterType
    {
        public SkullSmallType Type; public string Name() => "SkullSmall";
        public string TypeName() => $"{Name()}{Type}"; public SkullSmall(SkullSmallType type) { Type = type; }
    }
}