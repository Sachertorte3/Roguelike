using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Zombie : ICharacterType
    {
        public ZombieType Type; public string TypeName() => "Zombie";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Zombie(ZombieType type) { Type = type; }
    }
}