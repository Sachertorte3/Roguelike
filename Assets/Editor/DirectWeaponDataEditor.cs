using Domain.Model.Item;
using Domain.Service.Items;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Editor
{
    [CustomEditor(typeof(DirectWeaponData))]
    public class DirectWeaponDataEditor : OdinEditor
    {
        private float _evaluatedPrice;
        protected override void OnEnable()
        {
            base.OnEnable();
            _evaluatedPrice = EvaluatePrice();
        }
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            if (EditorGUI.EndChangeCheck())
            {
                _evaluatedPrice = EvaluatePrice();
            }
            EditorGUILayout.LabelField($"Evaluated Price: {_evaluatedPrice}G");
        }

        private float EvaluatePrice()
        {
            DirectWeaponData weaponData = (DirectWeaponData)target;
            DirectWeapon item = new DirectWeapon(weaponData);
            return item.EvaluatePrice();
        }
    }
}
