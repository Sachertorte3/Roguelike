using UnityEngine;
using UnityEngine.EventSystems;

namespace View.UI
{
    internal class AutoSelecter : MonoBehaviour
    {
        private void OnEnable()
        {
            if (EventSystem.current.currentSelectedGameObject == null)
                EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}