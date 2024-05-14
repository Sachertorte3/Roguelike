using UnityEngine;

namespace View
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteView : MonoBehaviour
    {
        private bool _isVisible;

        public Vector3 Position()
        {
            return transform.position;
        }

        public bool GetVisibility()
        {
            return _isVisible;
        }

        public void SetVisibility(bool visible)
        {
            _isVisible = visible;
            GetComponent<SpriteRenderer>().enabled = visible;
        }
    }
}