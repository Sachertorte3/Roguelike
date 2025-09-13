using System;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    [RequireComponent(typeof(Button))]
    public class ChoiceButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;

        public void Construct(string text, Action onSelect, Action onClick)
        {
            _text.text = text;
            GetComponent<Button>().OnSelectAsObservable().Subscribe(_ => onSelect());
            GetComponent<Button>().onClick.AddListener(onClick.Invoke);
        }
    }
}