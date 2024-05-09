using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Eel : ICharacterType
    {
        public EelType Type; public string Name() => "Eel";
        public string TypeName() => $"{Name()}{Type}"; public Eel(EelType type) { Type = type; }
    }
}