using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Tentacle : ICharacterType
    {
        public TentacleType Type;

        public Tentacle(TentacleType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Tentacle";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}