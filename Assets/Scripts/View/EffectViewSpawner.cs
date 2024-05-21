using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities.ObjectsManager;

namespace View
{
    public class EffectViewSpawner
    {
        private readonly GameObject _effect = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Effect.prefab")
            .WaitForCompletion();

        public void Spawn(IEnumerable<Vector2Int> area, int effectDisplayMilliseconds)
        {
            _effect.GetComponent<LifeTimer>().LifeTimeMilliseconds = effectDisplayMilliseconds;
            foreach (var position in area)
            {
                var spawnedEffect = Object.Instantiate(_effect);
                spawnedEffect.transform.position = (Vector3Int)position;
                spawnedEffect.GetComponent<SpriteView>().RegisterComponent();
            }
        }
    }
}