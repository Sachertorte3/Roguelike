using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Eye : ICharacterType
    {
        public EyeType Type; public string Name() => "Eye";
        public string TypeName() => $"{Name()}{Type}"; public Eye(EyeType type) { Type = type; }
    }
}