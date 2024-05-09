using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record FireSmall : ICharacterType
    {
        public FireSmallType Type; public string Name() => "FireSmall";
        public string TypeName() => $"{Name()}{Type}"; public FireSmall(FireSmallType type) { Type = type; }
    }
}