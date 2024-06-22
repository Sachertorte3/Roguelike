using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Zombie : ICharacterType
    {
        public ZombieType Type;

        public Zombie(ZombieType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Zombie";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}