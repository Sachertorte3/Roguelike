using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Chicken : ICharacterType
    {
        public ChickenType Type; public string TypeName() => "Chicken";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Chicken(ChickenType type) { Type = type; }
    }
}