using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Earth : ICharacterType
    {
        public EarthType Type;

        public Earth(EarthType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Earth";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}