using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Slime : ICharacterType
    {
        public SlimeType Type;

        public Slime(SlimeType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Slime";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}