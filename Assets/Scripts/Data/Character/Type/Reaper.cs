using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Reaper : ICharacterType
    {
        public ReaperType Type; public string TypeName() => "Reaper";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Reaper(ReaperType type) { Type = type; }
    }
}