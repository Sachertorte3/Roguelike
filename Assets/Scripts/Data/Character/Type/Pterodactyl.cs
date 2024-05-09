using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Pterodactyl : ICharacterType
    {
        public PterodactylType Type; public string Name() => "Pterodactyl";
        public string TypeName() => $"{Name()}{Type}"; public Pterodactyl(PterodactylType type) { Type = type; }
    }
}