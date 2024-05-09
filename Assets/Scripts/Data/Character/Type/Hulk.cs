using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Hulk : ICharacterType
    {
        public HulkType Type; public string TypeName() => "Hulk";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Hulk(HulkType type) { Type = type; }
    }
}