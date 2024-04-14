using UnityEngine;
using Unity.Entities;

class SpawnerAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public float SpawnRate;
}

class SpawnerBaker : Baker<SpawnerAuthoring>
{
    public override void Bake(SpawnerAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new Spawner
        {
            // デフォルトでは、各オーサリングGameObjectはEntityに変換されます。
            // GameObject（またはオーサリングコンポーネント）が与えられると、GetEntityは生成されるEntityを検索します。
            Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
            SpawnPosition = authoring.transform.position,
            NextSpawnTime = 0.0f,
            SpawnRate = authoring.SpawnRate
        });
    }
}