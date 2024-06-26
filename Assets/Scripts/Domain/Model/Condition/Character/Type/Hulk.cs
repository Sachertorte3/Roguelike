using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Hulk : ICharacterType
    {
        public HulkType Type;

        public Hulk(HulkType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Hulk";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}