using UnityEngine;

namespace UI
{
    public class CameraFollowTarget : MonoBehaviour
    {
        [SerializeField] GameObject target;
        [SerializeField] Vector3 relative;

        public void SetTarget(GameObject obj)
        {
            target = obj;
        }

        void LateUpdate()
        {
            if (target != null)
            {
                gameObject.transform.position = target.transform.position + relative;
            }
        }
    }
}