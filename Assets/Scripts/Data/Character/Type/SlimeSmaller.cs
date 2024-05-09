using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SlimeSmaller : ICharacterType
    {
        public SlimeSmallerType Type; public string Name() => "SlimeSmaller";
        public string TypeName() => $"{Name()}{Type}"; public SlimeSmaller(SlimeSmallerType type) { Type = type; }
    }
}