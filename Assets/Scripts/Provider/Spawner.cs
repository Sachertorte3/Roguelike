using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Spawner: MonoBehaviour
{
    [SerializeField] InputReceiver receiver;
    public void Start()
    {
        GameObject prefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion();
        GameObject view = Instantiate(prefab);
        receiver
            .MoveDirection
            .Subscribe(
            x => 
            view.
            GetComponent<CharacterView>().
            Move(
                DirectionMethods
                .FromVector(
                    x)))
            .AddTo(
            this);
    }
}