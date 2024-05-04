using Scripts.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scripts.View
{
    public class EffectViewer
    {
        public void Spawn(IEnumerable<Vector2Int> area, int effectDisplayMilliseconds)
        {
            GameObject effect = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Effect.prefab").WaitForCompletion();
            foreach (Vector2Int position in area)
            {
                GameObject spawnedEffect = GameObject.Instantiate(effect);
                spawnedEffect.transform.position = (Vector3Int)position;
                spawnedEffect.GetComponent<LifeTimer>().LifeTimeMilliseconds = effectDisplayMilliseconds;
            };
        }
    }
}
