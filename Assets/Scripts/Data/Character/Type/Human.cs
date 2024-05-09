using System;
using UnityEngine;

namespace Database.Characters.Type
{
    [Serializable]
    public record Human : ICharacterType
    {
        public Texture texture;
        public string Name() => "Human";
        public string TypeName() => texture.name;
    }
}