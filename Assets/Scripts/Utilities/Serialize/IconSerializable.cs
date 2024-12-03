#nullable enable
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Utilities.Serialize
{
    [Serializable]
    public class IconSerializable
    {
        [ShowInInspector] [OnValueChanged("OnValidate")]
        private Sprite _value;

        [ReadOnly] [SerializeField] private string _name;

        public Sprite Value
        {
            get
            {
                if (_value == null)
                {
                    _value = ScriptableObjectLoader.LoadIcon(_name);
                }

                return _value;
            }
        }

        public IconSerializable(Sprite value)
        {
            _value = value;
            _name = value.name;
        }

        private void OnValidate()
        {
            _name = _value.name;
        }
    }
}