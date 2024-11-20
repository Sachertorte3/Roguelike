using Unity.Logging;
using UnityEngine;
using UnityEngine.EventSystems;

namespace View.UI
{
    internal class AutoSelecter : MonoBehaviour
    {
        private void OnEnable()
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                Log.Info($"[Menu]AutoSelect: {gameObject}");
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
    }
}