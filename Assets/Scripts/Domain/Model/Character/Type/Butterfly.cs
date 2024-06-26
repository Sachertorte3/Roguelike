using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Butterfly : ICharacterType
    {
        public ButterflyType Type;

        public Butterfly(ButterflyType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Butterfly";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}