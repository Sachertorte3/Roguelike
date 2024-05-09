using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Lurker : ICharacterType
    {
        public LurkerType Type; public string TypeName() => "Lurker";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Lurker(LurkerType type) { Type = type; }
    }
}