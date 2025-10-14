using UnityEngine;
using Utilities;

namespace View
{
    [RequireComponent(typeof(SpriteRenderer), typeof(OverrideSprite))]
    public class CharacterDemoDisplay : MonoBehaviour, IDirectional
    {
        private string _textureName;

        public void SetTexture(string textureName)
        {
            Debug.Log("SetTexture: " + textureName);
            _textureName = textureName;
            GetComponent<OverrideSprite>().SetTexture("Human", _textureName, true);
        }

        public Direction8 GetDirection()
        {
            return Direction8.Down;
        }
    }
}