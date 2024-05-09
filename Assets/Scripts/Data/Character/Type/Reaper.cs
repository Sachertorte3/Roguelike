using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Reaper : ICharacterType
    {
        public ReaperType Type; public string Name() => "Reaper";
        public string TypeName() => $"{Name()}{Type}"; public Reaper(ReaperType type) { Type = type; }
    }
}