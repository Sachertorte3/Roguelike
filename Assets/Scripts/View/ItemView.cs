using UnityEngine;
using Utilities;

namespace View
{
    [RequireComponent(typeof(EntityView), typeof(ParticleController))]
    public class ItemView : MonoBehaviour
    {
        private bool _isShiny;
        public void SetShiny(bool value)
        {
            if (_isShiny == value) return;
            _isShiny = value;
            if (value)
                GetComponent<ParticleController>().Add(ParticleType.ShinyStar);
            else
                GetComponent<ParticleController>().Remove(ParticleType.ShinyStar);
        }
    }
}