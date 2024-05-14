#nullable enable
using System.IO;
using Data.Area;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Data
{
#if UNITY_EDITOR
#endif

    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Item")]
    public class ItemData : ScriptableObject, IHasInfo
    {
        [ReadOnly] public string Name = "";
        public Sprite Icon;
        [SerializeReference] public IEffect Effect;
        [SerializeReference] public IArea Area;
        [MinValue(1)] public int UsageLimit;
#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            Name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.SaveAssets();
        }
#endif
        public string Info()
        {
            return $"{Name}\n効果: {Effect.Info()}\n範囲: {Area.Info()}\n使用可能回数: {UsageLimit}";
        }
    }
}