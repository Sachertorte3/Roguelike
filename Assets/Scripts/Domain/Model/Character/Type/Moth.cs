using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Moth : ICharacterType
    {
        public MothType Type;

        public Moth(MothType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Moth";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}