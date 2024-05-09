using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Tentacle : ICharacterType
    {
        public TentacleType Type; public string Name() => "Tentacle";
        public string TypeName() => $"{Name()}{Type}"; public Tentacle(TentacleType type) { Type = type; }
    }
}