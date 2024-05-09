using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Hulk : ICharacterType
    {
        public HulkType Type; public string Name() => "Hulk";
        public string TypeName() => $"{Name()}{Type}"; public Hulk(HulkType type) { Type = type; }
    }
}