using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Cloud : ICharacterType
    {
        public CloudType Type; public string Name() => "Cloud";
        public string TypeName() => $"{Name()}{Type}"; public Cloud(CloudType type) { Type = type; }
    }
}