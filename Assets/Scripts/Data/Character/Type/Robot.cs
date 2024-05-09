using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Robot : ICharacterType
    {
        public RobotType Type; public string Name() => "Robot";
        public string TypeName() => $"{Name()}{Type}"; public Robot(RobotType type) { Type = type; }
    }
}