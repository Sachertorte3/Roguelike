using Data.Character.Type;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
#endif

namespace Data.Character
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [ReadOnly, Required] public string Name = "";
        [MinValue(1)] public int Hp;
        [MinValue(1)] public int Strength;
        [SerializeReference] public ICharacterType CharacterType;
        public Aggression Aggression = Aggression.AvoidAllies;
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