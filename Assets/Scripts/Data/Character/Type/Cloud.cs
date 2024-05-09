using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Cloud : ICharacterType
    {
        public CloudType Type; public string TypeName() => "Cloud";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Cloud(CloudType type) { Type = type; }
    }
}