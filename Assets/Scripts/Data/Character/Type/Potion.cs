using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Potion : ICharacterType
    {
        public PotionType Type; public string TypeName() => "Potion";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Potion(PotionType type) { Type = type; }
    }
}