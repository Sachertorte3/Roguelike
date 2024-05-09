using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Machine : ICharacterType
    {
        public MachineType Type; public string Name() => "Machine";
        public string TypeName() => $"{Name()}{Type}"; public Machine(MachineType type) { Type = type; }
    }
}