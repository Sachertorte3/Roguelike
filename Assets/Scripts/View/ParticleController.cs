using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace View
{
    public class ParticleController : MonoBehaviour
    {
        private Dictionary<ParticleType, int> _particleCounter = new();
        private Dictionary<ParticleType, GameObject> _particles = new();

        public void Add(ParticleType particleType)
        {
            if (_particles.ContainsKey(particleType))
            {
                _particleCounter[particleType]++;
            }
            else
            {
                var EffectPrefab = Addressables.LoadAssetAsync<GameObject>(particleType.GetPath()).WaitForCompletion();
                var particle = Instantiate(EffectPrefab, transform);
                _particles.Add(particleType, particle);
                _particleCounter.Add(particleType, 1);
                particle.SetActive(GetComponent<Renderer>().enabled);
            }
        }

        public void Remove(ParticleType particleType)
        {
            if (_particleCounter[particleType] > 1)
            {
                _particleCounter[particleType]--;
            }
            else
            {
                Destroy(_particles[particleType]);
                _particles.Remove(particleType);
                _particleCounter.Remove(particleType);
            }
        }
    }
}