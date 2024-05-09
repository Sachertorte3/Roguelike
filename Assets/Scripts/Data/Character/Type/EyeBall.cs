using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record EyeBall : ICharacterType
    {
        public EyeBallType Type; public string Name() => "EyeBall";
        public string TypeName() => $"{Name()}{Type}"; public EyeBall(EyeBallType type) { Type = type; }
    }
}