using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Mushroom : ICharacterType
    {
        public MushroomType Type; public string TypeName() => "Mushroom";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Mushroom(MushroomType type) { Type = type; }
    }
}