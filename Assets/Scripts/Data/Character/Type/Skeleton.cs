using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Skeleton : ICharacterType
    {
        public SkeletonType Type; public string Name() => "Skeleton";
        public string TypeName() => $"{Name()}{Type}"; public Skeleton(SkeletonType type) { Type = type; }
    }
}