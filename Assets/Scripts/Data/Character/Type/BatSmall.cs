using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record BatSmall : ICharacterType
    {
        public BatSmallType Type; public string TypeName() => "BatSmall";
        public string SubtypeName() => $"{TypeName()}{Type}"; public BatSmall(BatSmallType type) { Type = type; }
    }
}