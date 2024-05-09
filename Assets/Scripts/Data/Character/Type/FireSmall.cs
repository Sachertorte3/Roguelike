using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record FireSmall : ICharacterType
    {
        public FireSmallType Type; public string TypeName() => "FireSmall";
        public string SubtypeName() => $"{TypeName()}{Type}"; public FireSmall(FireSmallType type) { Type = type; }
    }
}