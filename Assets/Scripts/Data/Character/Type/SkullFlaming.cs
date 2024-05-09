using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SkullFlaming : ICharacterType
    {
        public SkullFlamingType Type; public string Name() => "SkullFlaming";
        public string TypeName() => $"{Name()}{Type}"; public SkullFlaming(SkullFlamingType type) { Type = type; }
    }
}