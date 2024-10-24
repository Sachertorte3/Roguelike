#nullable enable
using Sirenix.OdinInspector;
using UnityEngine;
using Domain.Model.Effect.Area;
using Domain.Model.Effect;
using Domain.Model.Effect.Position;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Domain.Model.Item
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/WeaponMold")]
    public class WeaponMold : ScriptableObject
    {
        [ReadOnly][Required] public string Name;
        [Required] public Sprite Icon;
        [MinValue(0)] public float PowerMagnification = 1;
        [MinValue(1)] public int UsageLimit;
        [SerializeReference][Required] public IArea Area;
        [SerializeReference][Required] public IEffectPosition Position = new AtFeet();
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