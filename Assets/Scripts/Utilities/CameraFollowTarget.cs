using UnityEngine;

namespace UI
{
    public class CameraFollowTarget : MonoBehaviour
    {
        [SerializeField] private GameObject target;
        [SerializeField] private Vector3 relative;

        public void SetTarget(GameObject obj)
        {
            target = obj;
        }

        private void LateUpdate()
        {
            if (target != null)
            {
                gameObject.transform.position = target.transform.position + relative;
            }
        }
    }
}