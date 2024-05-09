using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Machine : ICharacterType
    {
        public MachineType Type; public string TypeName() => "Machine";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Machine(MachineType type) { Type = type; }
    }
}