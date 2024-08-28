using TMPro;
using UnityEngine;

namespace View.UI
{
    public class DungeonInfoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _floorText;
        public void SetInfo(string name, int floor)
        {
            if (floor > 0)
            {
                _floorText.text = $"{name} B{floor}F";
            }
            else
            {
                _floorText.text = $"{name} {1 - floor}F";
            }
        }
    }
}