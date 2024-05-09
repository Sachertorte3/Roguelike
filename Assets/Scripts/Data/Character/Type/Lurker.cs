using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Lurker : ICharacterType
    {
        public LurkerType Type; public string Name() => "Lurker";
        public string TypeName() => $"{Name()}{Type}"; public Lurker(LurkerType type) { Type = type; }
    }
}