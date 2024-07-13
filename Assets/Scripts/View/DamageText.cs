using UnityEngine;

namespace UI
{
    public class DamageText : MonoBehaviour
    {
        private float speed = 1f / 240;

        private void Update()
        {
            transform.position = transform.position + new Vector3(0, speed, 0);
        }
    }
}