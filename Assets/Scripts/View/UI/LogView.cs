using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    public class LogView : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private TMP_Text _logTextPrefab;
        private int _logShownMilliSeconds = 3000;

        public void SetLogShownMilliSeconds(int milliSeconds)
        {
            _logShownMilliSeconds = milliSeconds;
        }

        public void AddLog(string log)
        {
            var logText = Instantiate(_logTextPrefab, _content);
            logText.text = log;
            logText.gameObject.AddComponent<LifeTimer>().LifeTimeMilliseconds = _logShownMilliSeconds;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)logText.transform);
        }

        public void Clear()
        {
            foreach (var logText in _content.GetComponentsInChildren<TMP_Text>())
            {
                Destroy(logText.gameObject);
            }
        }
    }
}