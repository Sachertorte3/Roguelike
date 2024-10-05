#nullable enable

using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Domain.Model.Item
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Material")]
    public class MaterialData : ScriptableObject
    {
        [ReadOnly] [Required] public string Name;
        [MinValue(1)] public int Power;
        [MinValue(0)] public float UsageLimitMagnification = 1;
#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            Name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}