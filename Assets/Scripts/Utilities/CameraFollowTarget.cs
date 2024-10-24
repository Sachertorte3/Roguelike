using UnityEngine;

namespace Utilities
{
    public class CameraFollowTarget : MonoBehaviour
    {
        [SerializeField] private GameObject target;
        [SerializeField] private Vector3 relative;

        private void LateUpdate()
        {
            if (target != null) gameObject.transform.position = target.transform.position + relative;
        }

        public void SetTarget(GameObject obj)
        {
            target = obj;
        }

        public void SetPosition(Vector3 position)
        {
            gameObject.transform.position = position + relative;
        }
    }
}