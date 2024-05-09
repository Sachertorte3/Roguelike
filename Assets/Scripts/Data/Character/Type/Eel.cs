using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Eel : ICharacterType
    {
        public EelType Type; public string TypeName() => "Eel";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Eel(EelType type) { Type = type; }
    }
}