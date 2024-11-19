using UnityEngine;

namespace Utilities
{
    [RequireComponent(typeof(Camera))]
    public class CameraFlameRect : MonoBehaviour
    {
        public void SetRect(RectInt rect)
        {
            gameObject.transform.position =
                new Vector3(rect.width / 2f, rect.height / 2f, gameObject.transform.position.z);
            GetComponent<Camera>().orthographicSize = Mathf.Max(rect.width, rect.height) / 2f;
        }
    }
}