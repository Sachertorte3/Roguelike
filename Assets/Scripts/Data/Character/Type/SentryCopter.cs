using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SentryCopter : ICharacterType
    {
        public SentryCopterType Type; public string Name() => "SentryCopter";
        public string TypeName() => $"{Name()}{Type}"; public SentryCopter(SentryCopterType type) { Type = type; }
    }
}