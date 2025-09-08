using UnityEngine;

namespace View
{
    public class SEManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _grassWalkSE;
        [SerializeField] private AudioClip _attackSE;
        [SerializeField] private AudioClip _pickupSE;
        [SerializeField] private AudioClip _stairsSE;
        [SerializeField] private AudioClip _teleportSE;

        public void SetVolume(float volume)
        {
            _audioSource.volume = volume;
        }

        public void GrassWalkSE()
        {
            _audioSource.PlayOneShot(_grassWalkSE);
        }

        public void AttackSE()
        {
            _audioSource.PlayOneShot(_attackSE);
        }

        public void PickupSE()
        {
            _audioSource.PlayOneShot(_pickupSE);
        }

        public void StairsSE()
        {
            _audioSource.PlayOneShot(_stairsSE);
        }

        public void TeleportSE()
        {
            _audioSource.PlayOneShot(_teleportSE);
        }
    }
}