using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Space : ICharacterType
    {
        public SpaceType Type; public string Name() => "Space";
        public string TypeName() => $"{Name()}{Type}"; public Space(SpaceType type) { Type = type; }
    }
}