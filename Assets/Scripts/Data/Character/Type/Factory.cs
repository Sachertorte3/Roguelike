using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Factory : ICharacterType
    {
        public FactoryType Type; public string TypeName() => "Factory";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Factory(FactoryType type) { Type = type; }
    }
}