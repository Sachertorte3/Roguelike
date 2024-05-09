using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Space : ICharacterType
    {
        public SpaceType Type; public string TypeName() => "Space";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Space(SpaceType type) { Type = type; }
    }
}