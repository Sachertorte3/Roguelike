using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Mouth : ICharacterType
    {
        public MouthType Type; public string TypeName() => "Mouth";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Mouth(MouthType type) { Type = type; }
    }
}