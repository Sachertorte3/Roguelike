using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Weasel : ICharacterType
    {
        public WeaselType Type; public string Name() => "Weasel";
        public string TypeName() => $"{Name()}{Type}"; public Weasel(WeaselType type) { Type = type; }
    }
}