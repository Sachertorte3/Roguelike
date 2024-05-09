using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Cat : ICharacterType
    {
        public CatType Type; public string Name() => "Cat";
        public string TypeName() => $"{Name()}{Type}"; public Cat(CatType type) { Type = type; }
    }
}