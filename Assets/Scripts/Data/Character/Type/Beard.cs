using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Beard : ICharacterType
    {
        public BeardType Type; public string Name() => "Beard";
        public string TypeName() => $"{Name()}{Type}"; public Beard(BeardType type) { Type = type; }
    }
}