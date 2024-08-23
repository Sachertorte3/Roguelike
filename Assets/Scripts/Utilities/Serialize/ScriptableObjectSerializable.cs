#nullable enable
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Domain.Model
{
    [Serializable]
    public class ScriptableObjectSerializable<T> where T : ScriptableObject
    {
        [ShowInInspector, OnValueChanged("OnValidate")] private T _value;
        [HideInInspector, SerializeField] private string _name;

        public T Value
        {
            get
            {
                if (_value == null)
                {
                    _value = Addressables.LoadAssetAsync<T>($"Assets/Database/{typeof(T).Name}/{_name}.asset").WaitForCompletion();
                }
                return _value;
            }
        }

        public ScriptableObjectSerializable(T value)
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