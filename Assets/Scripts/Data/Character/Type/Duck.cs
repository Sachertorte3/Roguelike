using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Duck : ICharacterType
    {
        public DuckType Type; public string Name() => "Duck";
        public string TypeName() => $"{Name()}{Type}"; public Duck(DuckType type) { Type = type; }
    }
}