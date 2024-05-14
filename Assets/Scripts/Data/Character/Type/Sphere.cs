using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Sphere : ICharacterType
    {
        public SphereType Type;

        public Sphere(SphereType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Sphere";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}