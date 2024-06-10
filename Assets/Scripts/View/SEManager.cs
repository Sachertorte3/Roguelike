using UnityEngine;

namespace View
{
    public class SEManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _attackSE;
        [SerializeField] private AudioClip _pickupSE;
        public void SetVolume(float volume)
        {
            _audioSource.volume = volume;
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

