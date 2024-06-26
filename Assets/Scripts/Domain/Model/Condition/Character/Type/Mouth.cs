using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Mouth : ICharacterType
    {
        public MouthType Type;

        public Mouth(MouthType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Mouth";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}