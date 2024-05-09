using System;
using UnityEngine;

namespace Database.Characters.Type
{
    [Serializable]
    public record Human : ICharacterType
    {
        public Texture Texture;
        public string TypeName() => "Human";
        public string SubtypeName() => Texture.name;
        public Human(Texture texture)
        {
            Texture = texture;
        }
    }
}