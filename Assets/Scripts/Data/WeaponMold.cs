#nullable enable
using Sirenix.OdinInspector;
using UnityEngine;
using Data.Area;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/WeaponMold")]
    public class WeaponMold : ScriptableObject
    {
        [ReadOnly] [Required] public string Name;
        [Required] public Sprite Icon;
        [MinValue(0)] public float PowerMagnification = 1;
        [MinValue(1)] public int UsageLimit;
        [Required] [SerializeReference] public IArea Area;
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