using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Coffin : ICharacterType
    {
        public CoffinType Type; public string Name() => "Coffin";
        public string TypeName() => $"{Name()}{Type}"; public Coffin(CoffinType type) { Type = type; }
    }
}