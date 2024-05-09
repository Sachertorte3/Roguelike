using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Potion : ICharacterType
    {
        public PotionType Type; public string Name() => "Potion";
        public string TypeName() => $"{Name()}{Type}"; public Potion(PotionType type) { Type = type; }
    }
}