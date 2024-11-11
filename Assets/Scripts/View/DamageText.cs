using UnityEngine;

namespace View
{
    public class DamageText : MonoBehaviour
    {
        private float speed = 1f / 240;

        private void Update()
        {
            transform.position += new Vector3(0, speed, 0);
        }
    }
}