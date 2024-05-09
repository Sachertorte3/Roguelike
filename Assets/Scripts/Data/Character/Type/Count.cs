using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Count : ICharacterType
    {
        public CountType Type; public string TypeName() => "Count";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Count(CountType type) { Type = type; }
    }
}