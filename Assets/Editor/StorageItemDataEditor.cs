using Domain.Model.Item;
using Domain.Service.Items;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Editor
{
    [CustomEditor(typeof(StorageItemData))]
    public class StorageItemDataEditor : OdinEditor
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
            StorageItemData itemData = (StorageItemData)target;
            StorageItem item = new StorageItem(itemData);
            return item.EvaluatePrice();
        }
    }
}
