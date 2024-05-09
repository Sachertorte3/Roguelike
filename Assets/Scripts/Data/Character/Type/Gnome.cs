using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Gnome : ICharacterType
    {
        public GnomeType Type; public string Name() => "Gnome";
        public string TypeName() => $"{Name()}{Type}"; public Gnome(GnomeType type) { Type = type; }
    }
}