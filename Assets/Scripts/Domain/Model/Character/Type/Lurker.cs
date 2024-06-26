using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Lurker : ICharacterType
    {
        public LurkerType Type;

        public Lurker(LurkerType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Lurker";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}