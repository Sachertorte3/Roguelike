using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Head : ICharacterType
    {
        public HeadType Type; public string TypeName() => "Head";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Head(HeadType type) { Type = type; }
    }
}