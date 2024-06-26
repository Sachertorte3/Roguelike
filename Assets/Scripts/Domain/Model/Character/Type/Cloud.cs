using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Cloud : ICharacterType
    {
        public CloudType Type;

        public Cloud(CloudType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Cloud";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}