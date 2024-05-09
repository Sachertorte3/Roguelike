using System;
using UnityEngine;

namespace Database.Characters.Type
{
    [Serializable]
    public record Human : ICharacterType
    {
        public Texture texture;
        public string TypeName() => "Human";
        public string SubtypeName() => texture.name;
    }
}