using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Skeleton : ICharacterType
    {
        public SkeletonType Type; public string TypeName() => "Skeleton";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Skeleton(SkeletonType type) { Type = type; }
    }
}