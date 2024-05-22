using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    public class LogView : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _content;
        [SerializeField] private TMP_Text _logTextPrefab;
        public void AddLog(string log)
        {
            var logText = Instantiate(_logTextPrefab, _content);
            logText.text = log;
        }
        private void Update()
        {
            _scrollRect.verticalNormalizedPosition /= 1.2f;
        }
    }
}
