using Domain.Model.Item;
using Domain.Service.Items;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : OdinEditor
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
        ItemData itemData = (ItemData)target;
        Item item = new Item(itemData, "Test");
        return item.EvaluatePrice();
    }
}
