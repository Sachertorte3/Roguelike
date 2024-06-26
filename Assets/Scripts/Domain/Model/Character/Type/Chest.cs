using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Chest : ICharacterType
    {
        public ChestType Type;

        public Chest(ChestType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Chest";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}