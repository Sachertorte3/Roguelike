using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Mushroom : ICharacterType
    {
        public MushroomType Type; public string Name() => "Mushroom";
        public string TypeName() => $"{Name()}{Type}"; public Mushroom(MushroomType type) { Type = type; }
    }
}