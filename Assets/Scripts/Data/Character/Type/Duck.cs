using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Duck : ICharacterType
    {
        public DuckType Type; public string TypeName() => "Duck";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Duck(DuckType type) { Type = type; }
    }
}