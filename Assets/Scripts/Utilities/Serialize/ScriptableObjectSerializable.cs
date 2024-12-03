#nullable enable
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Utilities.Serialize
{
    [Serializable]
    public class ScriptableObjectSerializable<T> where T : ScriptableObject
    {
        [ShowInInspector] [OnValueChanged("OnValidate")]
        private T _value;

        [ReadOnly] [SerializeField] private string _name;

        public T Value
        {
            get
            {
                if (_value == null)
                {
                    _value = ScriptableObjectLoader.Load<T>(_name);
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