using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Pterodactyl : ICharacterType
    {
        public PterodactylType Type;

        public Pterodactyl(PterodactylType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Pterodactyl";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}