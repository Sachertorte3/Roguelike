using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Gnome : ICharacterType
    {
        public GnomeType Type;

        public Gnome(GnomeType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Gnome";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}