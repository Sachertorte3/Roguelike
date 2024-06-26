using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Machine : ICharacterType
    {
        public MachineType Type;

        public Machine(MachineType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Machine";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}