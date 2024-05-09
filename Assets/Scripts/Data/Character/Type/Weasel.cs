using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Weasel : ICharacterType
    {
        public WeaselType Type; public string TypeName() => "Weasel";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Weasel(WeaselType type) { Type = type; }
    }
}