using Scripts.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scripts.View
{
    public class EffectViewSpawner
    {
        public void Spawn(IEnumerable<Vector2Int> area, int effectDisplayMilliseconds)
        {
            GameObject effect = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Effect.prefab").WaitForCompletion();
            effect.GetComponent<LifeTimer>().LifeTimeMilliseconds = effectDisplayMilliseconds;
            foreach (Vector2Int position in area)
            {
                GameObject spawnedEffect = GameObject.Instantiate(effect);
                spawnedEffect.transform.position = (Vector3Int)position;
                spawnedEffect.GetComponent<SpriteView>().RegisterComponent();
            };
        }
    }
}
