using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Sphere : ICharacterType
    {
        public SphereType Type; public string Name() => "Sphere";
        public string TypeName() => $"{Name()}{Type}"; public Sphere(SphereType type) { Type = type; }
    }
}