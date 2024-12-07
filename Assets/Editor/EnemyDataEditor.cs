using Domain.Model.Character;
using Domain.Service.Characters;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Editor
{
    [CustomEditor(typeof(EnemyData))]
    public class EnemyDataEditor : OdinEditor
    {
        private float _evaluatedExp;
        protected override void OnEnable()
        {
            base.OnEnable();
            _evaluatedExp = CharacterFactory.EvaluateExp((EnemyData)target, false);
        }
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            if (EditorGUI.EndChangeCheck())
            {
                _evaluatedExp = CharacterFactory.EvaluateExp((EnemyData)target, false);
            }
            EditorGUILayout.LabelField($"Evaluated Exp: {_evaluatedExp}Exp");
        }
    }
}
