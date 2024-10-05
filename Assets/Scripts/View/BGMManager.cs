using Unity.Logging;
using UnityEngine;

namespace View
{
    public class BGMManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _normalBGM;
        [SerializeField] private AudioClip _stolenBGM;

        public void SetVolume(float volume)
        {
            _audioSource.volume = volume;
        }

        public void NormalBGM()
        {
            Log.Debug("Change BGM to Normal");
            ChangeBGM(_normalBGM);
        }

        public void StolenBGM()
        {
            Log.Debug("Change BGM to Stolen");
            ChangeBGM(_stolenBGM);
        }

        public void ChangeBGM(AudioClip clip)
        {
            if (_audioSource.clip == clip)
            {
                Debug.Log(clip.name);
                return;
            }

            _audioSource.clip = clip;
            _audioSource.Play();
        }
    }
}