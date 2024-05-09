using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SkullFlaming : ICharacterType
    {
        public SkullFlamingType Type; public string TypeName() => "SkullFlaming";
        public string SubtypeName() => $"{TypeName()}{Type}"; public SkullFlaming(SkullFlamingType type) { Type = type; }
    }
}