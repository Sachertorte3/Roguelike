using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Ghast : ICharacterType
    {
        public GhastType Type; public string TypeName() => "Ghast";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Ghast(GhastType type) { Type = type; }
    }
}