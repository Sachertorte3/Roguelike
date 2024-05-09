using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Chest : ICharacterType
    {
        public ChestType Type; public string Name() => "Chest";
        public string TypeName() => $"{Name()}{Type}"; public Chest(ChestType type) { Type = type; }
    }
}