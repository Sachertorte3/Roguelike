using R3;
using Scripts.Utilities;
using UnityEngine;

namespace Scripts.View
{
    public class CharacterArrow : MonoBehaviour
    {
        public void Constract(CharacterView view)
        {
            var direction = view.Direction;
            Turn(direction.CurrentValue);
            direction.Subscribe(direction => Turn(direction));
        }
        private void Turn(Direction8 direction)
        {
            transform.localEulerAngles = new Vector3(0, 0, direction.Angle().Value - 90);
        }
    }
}
