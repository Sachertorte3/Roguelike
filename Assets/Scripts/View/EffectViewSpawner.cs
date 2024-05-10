using Scripts.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scripts.View
{
    public class EffectViewSpawner
    {
        private GameObject _effect = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Effect.prefab").WaitForCompletion();
        public void Spawn(IEnumerable<Vector2Int> area, int effectDisplayMilliseconds)
        {
            _effect.GetComponent<LifeTimer>().LifeTimeMilliseconds = effectDisplayMilliseconds;
            foreach (Vector2Int position in area)
            {
                GameObject spawnedEffect = GameObject.Instantiate(_effect);
                spawnedEffect.transform.position = (Vector3Int)position;
                spawnedEffect.GetComponent<SpriteView>().RegisterComponent();
            };
        }
    }
}
