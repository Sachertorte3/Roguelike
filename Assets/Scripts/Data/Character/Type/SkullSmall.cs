using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SkullSmall : ICharacterType
    {
        public SkullSmallType Type; public string TypeName() => "SkullSmall";
        public string SubtypeName() => $"{TypeName()}{Type}"; public SkullSmall(SkullSmallType type) { Type = type; }
    }
}