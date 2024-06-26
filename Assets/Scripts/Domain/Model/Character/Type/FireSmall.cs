using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record FireSmall : ICharacterType
    {
        public FireSmallType Type;

        public FireSmall(FireSmallType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "FireSmall";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}