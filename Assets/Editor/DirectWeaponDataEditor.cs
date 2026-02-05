using Domain.Model.Item;
using Domain.Service.Items;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine.AddressableAssets;

namespace Editor
{
    [CustomEditor(typeof(DirectWeaponData))]
    public class DirectWeaponDataEditor : OdinEditor
    {
        private float _evaluatedPrice;
        private float _marketPrice;
        private static ItemMarketPriceTable? _cachedMarketPriceTable;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            if (_cachedMarketPriceTable == null)
            {
                _cachedMarketPriceTable = Addressables.LoadAssetAsync<ItemMarketPriceTable>("Assets/Database/ItemData/ItemMarketPriceTable.asset")
                    .WaitForCompletion();
            }
            _evaluatedPrice = EvaluateEvaluatedPrice();
            _marketPrice = EvaluateMarketPrice();
        }
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            if (EditorGUI.EndChangeCheck())
            {
                _evaluatedPrice = EvaluateEvaluatedPrice();
                _marketPrice = EvaluateMarketPrice();
            }
            EditorGUILayout.LabelField($"Evaluated Price: {_evaluatedPrice}G");
            EditorGUILayout.LabelField($"Market Price: {_marketPrice}G");
        }

        private float EvaluateEvaluatedPrice()
        {
            DirectWeaponData weaponData = (DirectWeaponData)target;
            DirectWeapon item = new DirectWeapon(weaponData);
            return item.EvaluateEvaluatedPrice();
        }

        private float EvaluateMarketPrice()
        {
            DirectWeaponData weaponData = (DirectWeaponData)target;
            DirectWeapon item = new DirectWeapon(weaponData);
            return item.EvaluatePrice(_cachedMarketPriceTable!);
        }
    }
}
