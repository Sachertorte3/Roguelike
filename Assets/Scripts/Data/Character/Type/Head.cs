using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Head : ICharacterType
    {
        public HeadType Type; public string Name() => "Head";
        public string TypeName() => $"{Name()}{Type}"; public Head(HeadType type) { Type = type; }
    }
}