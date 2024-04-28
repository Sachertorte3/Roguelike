using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Spawner: MonoBehaviour
{
    public void Start()
    {
        GameObject view = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion();
        Instantiate(view);
    }
}