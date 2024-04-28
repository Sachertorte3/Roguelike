using Scripts.Utilities;
using UnityEngine;

namespace Scripts.View
{
    public class CharacterView : MonoBehaviour
    {
        public void Move(Direction8 direction)
        {
            GetComponent<Transform>().transform.position += (Vector3Int)direction.Vector();
        }
    }
}