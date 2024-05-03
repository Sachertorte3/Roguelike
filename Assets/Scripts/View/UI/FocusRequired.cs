using UnityEngine;
using UnityEngine.EventSystems;

// 追加

namespace UI
{
    public class FocusRequired : MonoBehaviour
    {
        /// <summary>選択させないオブジェクト一覧。</summary>
        [SerializeField] private GameObject[] NotSelectables;

        /// <summary>直前まで選択されていたオブジェクト。</summary>
        private GameObject PreviousSelection = null;

        /// <summary>
        /// 選択対象のオブジェクト一覧。
        /// </summary>
        private GameObject[] _selectables;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Update()
        {
            if (EventSystem.current? EventSystem.current.currentSelectedGameObject != PreviousSelection : false)
            {
                if (EventSystem.current.currentSelectedGameObject == null)
                {
                    EventSystem.current.SetSelectedGameObject(PreviousSelection);
                }
                else
                {
                    PreviousSelection = EventSystem.current.currentSelectedGameObject;
                }
            }
        }
    }
}