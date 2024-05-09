using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SentryCopter : ICharacterType
    {
        public SentryCopterType Type; public string TypeName() => "SentryCopter";
        public string SubtypeName() => $"{TypeName()}{Type}"; public SentryCopter(SentryCopterType type) { Type = type; }
    }
}