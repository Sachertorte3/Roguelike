using Scripts.Utilities;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scripts.View
{
    [RequireComponent(typeof(SpriteRenderer), typeof(IDirectional))]
    public class OverrideSprite : MonoBehaviour
    {
        private SpriteRenderer sr;
        private IDirectional character;
        private bool _isDirectionalTexture;
        private string _textureSubtypeName;
        private string _textureTypeName;

        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            character = GetComponent<CharacterView>();
        }
        public void SetTexture(string textureTypeName, string textureSubtypeName)
        {
            _textureTypeName = textureTypeName;
            _textureSubtypeName = textureSubtypeName;
        }
        private void LateUpdate()
        {
            if (sr.enabled)
            {
                sprChange();
            }
        }
        private void sprChange()
        {
            string sprName = sr.sprite.name;
            int pos = sprName.LastIndexOf('_');
            if (pos >= 0)
            {
                Sprite sprite = GetSprite(sprName, pos);
                if (sprite != null)
                {
                    sr.sprite = sprite;
                }
            }
        }
        private Sprite GetSprite(string spriteName, int index)
        {
            if (_textureSubtypeName == null)
            {
                return null;
            }
            if (_isDirectionalTexture)
            {
                Direction8 direction = character.GetDirection();
                int id = int.Parse(spriteName.Substring(index + 1)) + GetIndex(direction);
                return Addressables
                    .LoadAssetAsync<Sprite>(
                        $"Assets/Images/Characters/{_textureSubtypeName}.png[{_textureSubtypeName}_{id}]")
                    .WaitForCompletion();
            }
            else
            {
                Direction8 direction = character.GetDirection();
                sr.flipX = NeedFlip(direction);
                string post = spriteName.Substring(index);
                return Addressables
                    .LoadAssetAsync<Sprite>(
                        $"Assets/Images/Monsters/{_textureSubtypeName}.png[{_textureTypeName + post}]")
                    .WaitForCompletion();
            }
        }

        public int GetIndex(Direction8 direction)
        {
            switch (direction)
            {
                case Direction8.Left: return 6;
                case Direction8.Right: return 12;
                case Direction8.Up: return 18;
                case Direction8.Down: return 0;
                case Direction8.UpLeft: return 15;
                case Direction8.DownLeft: return 3;
                case Direction8.UpRight: return 21;
                case Direction8.DownRight: return 9;
                default: return default;
            }
        }
        public bool NeedFlip(Direction8 direction)
        {
            switch (direction)
            {
                case Direction8.Left: return false;
                case Direction8.Right: return true;
                case Direction8.Up: return true;
                case Direction8.Down: return false;
                case Direction8.UpLeft: return false;
                case Direction8.DownLeft: return false;
                case Direction8.UpRight: return true;
                case Direction8.DownRight: return true;
                default: throw new InvalidEnumArgumentException();
            }
        }
    }
    public interface IDirectional
    {
        public Direction8 GetDirection();
    }
}