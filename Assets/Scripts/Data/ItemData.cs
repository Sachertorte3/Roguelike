#nullable enable
using UnityEngine;
using Sirenix.OdinInspector;

namespace Scripts.Data
{
#if UNITY_EDITOR
    using Sirenix.OdinInspector.Editor;
    using System.IO;
    using UnityEditor;

    [CustomEditor(typeof(ItemData))]
    public class ExampleEditor : OdinEditor
    {
        public override Texture2D RenderStaticPreview
        (
            string assetPath,
            Object[] subAssets,
            int width,
            int height
        )
        {
            var obj = target as ItemData;
            var icon = obj.Icon;

            if (icon == null)
            {
                return base.RenderStaticPreview(assetPath, subAssets, width, height);
            }

            var preview = AssetPreview.GetAssetPreview(icon);
            var final = new Texture2D(width, height);

            EditorUtility.CopySerialized(preview, final);

            return final;
        }
    }

#endif
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Item")]
    public class ItemData : ScriptableObject
    {
        [ReadOnly] public string Name = "";
        public Sprite Icon;
        public SkillData Skill;
        [MinValue(1)] public int UsageLimit;
        private void OnValidate()
        {
            string assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            Name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.SaveAssets();
        }
    }
}
