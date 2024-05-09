using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Hare : ICharacterType
    {
        public HareType Type; public string Name() => "Hare";
        public string TypeName() => $"{Name()}{Type}"; public Hare(HareType type) { Type = type; }
    }
}