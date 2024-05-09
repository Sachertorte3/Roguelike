using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Count : ICharacterType
    {
        public CountType Type; public string Name() => "Count";
        public string TypeName() => $"{Name()}{Type}"; public Count(CountType type) { Type = type; }
    }
}