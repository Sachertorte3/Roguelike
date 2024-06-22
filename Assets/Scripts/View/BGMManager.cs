using UnityEngine;

namespace View
{
    public class BGMManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;

        public void SetVolume(float volume)
        {
            _audioSource.volume = volume;
        }
    }
}