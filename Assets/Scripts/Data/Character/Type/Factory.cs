using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Factory : ICharacterType
    {
        public FactoryType Type; public string Name() => "Factory";
        public string TypeName() => $"{Name()}{Type}"; public Factory(FactoryType type) { Type = type; }
    }
}