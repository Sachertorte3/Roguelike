using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine;
using Utilities;

namespace Assets.Scripts.View
{
    public class ParticleController : MonoBehaviour
    {
        Dictionary<ParticleType, GameObject> _particles = new();
        Dictionary<ParticleType, int> _particleCounter = new();

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
