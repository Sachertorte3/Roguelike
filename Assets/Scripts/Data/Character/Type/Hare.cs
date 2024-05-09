using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Hare : ICharacterType
    {
        public HareType Type; public string TypeName() => "Hare";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Hare(HareType type) { Type = type; }
    }
}