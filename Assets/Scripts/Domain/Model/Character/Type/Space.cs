using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Space : ICharacterType
    {
        public SpaceType Type;

        public Space(SpaceType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Space";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}