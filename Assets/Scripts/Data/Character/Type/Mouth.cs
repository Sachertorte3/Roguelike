using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Mouth : ICharacterType
    {
        public MouthType Type; public string Name() => "Mouth";
        public string TypeName() => $"{Name()}{Type}"; public Mouth(MouthType type) { Type = type; }
    }
}