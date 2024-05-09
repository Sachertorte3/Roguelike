using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record EyeBall : ICharacterType
    {
        public EyeBallType Type; public string TypeName() => "EyeBall";
        public string SubtypeName() => $"{TypeName()}{Type}"; public EyeBall(EyeBallType type) { Type = type; }
    }
}