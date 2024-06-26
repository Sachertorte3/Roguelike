using System;
using UnityEngine;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Human : ICharacterType
    {
        public Texture Texture;

        public Human(Texture texture)
        {
            Texture = texture;
        }

        public string TypeName()
        {
            return "Human";
        }

        public string SubtypeName()
        {
            return Texture.name;
        }
    }
}