using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Dog : ICharacterType
    {
        public DogType Type; public string TypeName() => "Dog";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Dog(DogType type) { Type = type; }
    }
}