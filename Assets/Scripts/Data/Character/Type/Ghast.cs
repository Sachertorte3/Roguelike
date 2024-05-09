using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Ghast : ICharacterType
    {
        public GhastType Type; public string Name() => "Ghast";
        public string TypeName() => $"{Name()}{Type}"; public Ghast(GhastType type) { Type = type; }
    }
}