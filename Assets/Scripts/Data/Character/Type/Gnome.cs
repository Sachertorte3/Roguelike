using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Gnome : ICharacterType
    {
        public GnomeType Type; public string TypeName() => "Gnome";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Gnome(GnomeType type) { Type = type; }
    }
}