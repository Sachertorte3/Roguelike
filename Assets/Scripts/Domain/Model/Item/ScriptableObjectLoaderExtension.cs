using UnityEngine;
using Utilities;

namespace Domain.Model.Item
{
    public static class ScriptableObjectLoaderExtension
    {
        public static IItemData LoadItemData(string name)
        {
            return (IItemData)ScriptableObjectLoader.LoadWithPath<ScriptableObject>($"Assets/Database/ItemData/{name}.asset");
        }
    }
}