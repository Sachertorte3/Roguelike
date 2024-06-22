using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Skeleton : ICharacterType
    {
        public SkeletonType Type;

        public Skeleton(SkeletonType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Skeleton";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}