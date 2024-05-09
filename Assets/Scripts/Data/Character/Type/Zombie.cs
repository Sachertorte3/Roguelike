using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Zombie : ICharacterType
    {
        public ZombieType Type; public string Name() => "Zombie";
        public string TypeName() => $"{Name()}{Type}"; public Zombie(ZombieType type) { Type = type; }
    }
}