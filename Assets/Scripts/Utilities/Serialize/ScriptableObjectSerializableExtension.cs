#nullable enable
using UnityEngine;

namespace Utilities.Serialize
{
    public static class ScriptableObjectSerializableExtension
    {
        public static ScriptableObjectSerializable<T> ToSerializable<T>(this T value) where T : ScriptableObject
        {
            return new ScriptableObjectSerializable<T>(value);
        }
    }
}