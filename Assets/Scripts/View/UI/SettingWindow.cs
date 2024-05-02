using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.View.UI
{
    internal class SettingWindow: MonoBehaviour
    {
        [SerializeField] Transform _content;
        [SerializeField] SliderItem _sliderItem;
        private void Start()
        {
            AddValueItem("BGM音量(ダミー)", 0, 100, 50);
            AddValueItem("SE音量(ダミー)", 0, 100, 50);
        }
        public void AddValueItem(string itemName, int min, int max, int value)
        {
            _sliderItem.SetData(itemName, min, max, value);
            GameObject.Instantiate(_sliderItem, _content);
        }
    }
}
