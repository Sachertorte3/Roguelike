using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Bug : ICharacterType
    {
        public BugType Type; public string Name() => "Bug";
        public string TypeName() => $"{Name()}{Type}"; public Bug(BugType type) { Type = type; }
    }
}