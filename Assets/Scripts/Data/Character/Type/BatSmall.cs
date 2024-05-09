using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record BatSmall : ICharacterType
    {
        public BatSmallType Type; public string Name() => "BatSmall";
        public string TypeName() => $"{Name()}{Type}"; public BatSmall(BatSmallType type) { Type = type; }
    }
}