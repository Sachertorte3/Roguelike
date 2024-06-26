using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Duck : ICharacterType
    {
        public DuckType Type;

        public Duck(DuckType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Duck";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}