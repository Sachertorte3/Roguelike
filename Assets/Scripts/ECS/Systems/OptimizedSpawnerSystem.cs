using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public partial struct SpriteGenerator : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        // Spawnerの全Componentを問い合わせる。
        // このSystemでは、Componentへの読み取りと書き込みを行いたいので、RefRWを使用する。
        // Systemが読み取り専用のみを要するなら、RefROを使用する。
        ProcessSpawner(ref state);
    }

    private void ProcessSpawner(ref SystemState state)
    {
        // Spawnerの位置に新しいEntityを生成する。
        Entity newEntity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponent(newEntity, typeof(Character));
        state.EntityManager.AddComponent(newEntity, typeof(LocalTransform));
        state.EntityManager.AddComponent(newEntity, typeof(SpriteRenderer));
    }
}