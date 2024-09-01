using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Human : ICharacterType
    {
        public string TextureName;

        public Human(string textureName)
        {
            TextureName = textureName;
        }

        public string TypeName()
        {
            return "Human";
        }

        public string SubtypeName()
        {
            return TextureName;
        }
    }
}