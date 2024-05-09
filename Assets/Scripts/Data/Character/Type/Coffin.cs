using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Coffin : ICharacterType
    {
        public CoffinType Type; public string TypeName() => "Coffin";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Coffin(CoffinType type) { Type = type; }
    }
}