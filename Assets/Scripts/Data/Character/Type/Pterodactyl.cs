using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Pterodactyl : ICharacterType
    {
        public PterodactylType Type; public string TypeName() => "Pterodactyl";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Pterodactyl(PterodactylType type) { Type = type; }
    }
}