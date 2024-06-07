using UnityEngine;

namespace View
{
    [RequireComponent(typeof(AudioSource))]
    public class SEManager: MonoBehaviour
    {
        private AudioSource _audioSource;
        [SerializeField] private AudioClip _attackSE;
        [SerializeField] private AudioClip _pickupSE;
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }
        public void AttackSE()
        {
            _audioSource.PlayOneShot(_attackSE);
        }
        public void PickupSE()
        {
            _audioSource.PlayOneShot(_pickupSE);
        }
    }
}

