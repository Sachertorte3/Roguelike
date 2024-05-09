using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Chicken : ICharacterType
    {
        public ChickenType Type; public string Name() => "Chicken";
        public string TypeName() => $"{Name()}{Type}"; public Chicken(ChickenType type) { Type = type; }
    }
}