using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Robot : ICharacterType
    {
        public RobotType Type;

        public Robot(RobotType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Robot";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}