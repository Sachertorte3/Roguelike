using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Dog : ICharacterType
    {
        public DogType Type; public string Name() => "Dog";
        public string TypeName() => $"{Name()}{Type}"; public Dog(DogType type) { Type = type; }
    }
}