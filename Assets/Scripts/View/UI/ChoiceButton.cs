using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    [RequireComponent(typeof(Button))]
    public class ChoiceButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        public void Construct(string text, Action onClick)
        {
            _text.text = text;
            GetComponent<Button>().onClick.AddListener(() =>
            {
                onClick();
            });
        }
    }
}