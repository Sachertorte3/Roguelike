using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Bug : ICharacterType
    {
        public BugType Type; public string TypeName() => "Bug";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Bug(BugType type) { Type = type; }
    }
}