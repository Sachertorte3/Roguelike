using UnityEngine;

namespace Scripts.View
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteView : MonoBehaviour
    {
        private bool _isVisible = false;
        public Vector3 Position() => transform.position;
        public bool GetVisibility() => _isVisible;
        public void SetVisibility(bool visible)
        {
            _isVisible = visible;
            GetComponent<SpriteRenderer>().enabled = visible;
        }
    }
}
