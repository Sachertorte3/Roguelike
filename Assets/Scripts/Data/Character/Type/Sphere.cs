using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Sphere : ICharacterType
    {
        public SphereType Type; public string TypeName() => "Sphere";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Sphere(SphereType type) { Type = type; }
    }
}