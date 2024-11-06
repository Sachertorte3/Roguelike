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
        [HideInInspector] [SerializeField] private int _sortingLayerID;

        public void Add(ParticleType particleType)
        {
            if (particleType == ParticleType.None)
                return;

            if (_particles.ContainsKey(particleType))
            {
                _particleCounter[particleType]++;
            }
            else
            {
                var EffectPrefab = Addressables.LoadAssetAsync<GameObject>(particleType.GetPath()).WaitForCompletion();
                var particle = Instantiate(EffectPrefab, transform);
                foreach (var particleSystem in particle.GetComponentsInChildren<ParticleSystem>())
                {
                    particleSystem.gameObject.layer = gameObject.layer;
                    particleSystem.GetComponent<Renderer>().sortingLayerID = _sortingLayerID;
                }

                _particles.Add(particleType, particle);
                _particleCounter.Add(particleType, 1);
            }
        }

        public void Remove(ParticleType particleType)
        {
            if (particleType == ParticleType.None)
                return;

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

        public void Clear()
        {
            foreach (var particle in _particles)
            {
                Destroy(particle.Value);
            }

            _particles.Clear();
            _particleCounter.Clear();
        }
    }
}