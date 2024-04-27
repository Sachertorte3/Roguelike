using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Spawner: MonoBehaviour
{
    public void Start()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity newEntity = entityManager.CreateEntity();
        entityManager.AddComponent<LocalTransform>(newEntity);
        GameObject view = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion();
        Instantiate(view);
    }
}