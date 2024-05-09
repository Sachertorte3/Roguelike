using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Beard : ICharacterType
    {
        public BeardType Type; public string TypeName() => "Beard";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Beard(BeardType type) { Type = type; }
    }
}