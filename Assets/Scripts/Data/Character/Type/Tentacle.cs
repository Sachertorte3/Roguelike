using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Tentacle : ICharacterType
    {
        public TentacleType Type; public string TypeName() => "Tentacle";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Tentacle(TentacleType type) { Type = type; }
    }
}