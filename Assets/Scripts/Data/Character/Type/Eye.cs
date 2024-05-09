using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Eye : ICharacterType
    {
        public EyeType Type; public string TypeName() => "Eye";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Eye(EyeType type) { Type = type; }
    }
}