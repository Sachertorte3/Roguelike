using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace View
{
    public class EffectViewSpawner
    {
        private readonly GameObject _effect = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Effect.prefab")
            .WaitForCompletion();

        public void Spawn(IEnumerable<Vector2Int> area, Color color, int effectDisplayMilliseconds)
        {
            foreach (var position in area)
            {
                var spawnedEffect = Object.Instantiate(_effect);
                spawnedEffect.AddComponent<LifeTimer>().LifeTimeMilliseconds = effectDisplayMilliseconds;
                spawnedEffect.transform.position =
                    new Vector3(position.x, position.y, spawnedEffect.transform.position.z);
                spawnedEffect.GetComponent<SpriteRenderer>().color = color;
            }
        }

        public List<GameObject> SpawnPreview(IEnumerable<Vector2Int> area, Color color)
        {
            var spawnedEffects = new List<GameObject>();
            foreach (var position in area)
            {
                var spawnedEffect = Object.Instantiate(_effect);
                spawnedEffect.transform.position =
                    new Vector3(position.x, position.y, spawnedEffect.transform.position.z);
                spawnedEffect.GetComponent<SpriteRenderer>().color = color;
                spawnedEffects.Add(spawnedEffect);
            }

            return spawnedEffects;
        }
    }
}