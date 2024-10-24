#nullable enable
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Domain.Model
{
    [Serializable]
    public class IconSerializable
    {
        [ShowInInspector, OnValueChanged("OnValidate")] private Sprite _value;
        [HideInInspector, SerializeField] private string _name;

        public Sprite Value
        {
            get
            {
                if (_value == null)
                {
                    _value = Addressables.LoadAssetAsync<Sprite>($"Assets/Images/icons_full_16.png[{_name}]").WaitForCompletion();
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