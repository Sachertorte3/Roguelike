using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Witch : ICharacterType
    {
        public WitchType Type;

        public Witch(WitchType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Witch";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}