using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record SlimeSmaller : ICharacterType
    {
        public SlimeSmallerType Type; public string TypeName() => "SlimeSmaller";
        public string SubtypeName() => $"{TypeName()}{Type}"; public SlimeSmaller(SlimeSmallerType type) { Type = type; }
    }
}