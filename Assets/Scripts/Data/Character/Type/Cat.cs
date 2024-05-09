using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Cat : ICharacterType
    {
        public CatType Type; public string TypeName() => "Cat";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Cat(CatType type) { Type = type; }
    }
}