using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UniRx;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.View.UI
{
    internal class SliderText: MonoBehaviour
    {
        [SerializeField] Slider _slider;
        [SerializeField] TMP_Text _text;
        private void Start()
        {
            _text.SetText(_slider.value.ToString());
            _slider.onValueChanged.AsObservable().Subscribe(value =>
            {
                _text.SetText(value.ToString());
            });
        }
    }
}
