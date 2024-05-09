using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Robot : ICharacterType
    {
        public RobotType Type; public string TypeName() => "Robot";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Robot(RobotType type) { Type = type; }
    }
}