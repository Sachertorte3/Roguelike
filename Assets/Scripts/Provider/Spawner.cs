using Scripts.Model;
using Scripts.Utilities;
using Scripts.View;
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
        Character character = new Character();
        character.MoveSubject.Subscribe(direction => view.GetComponent<CharacterView>().Move(direction));
        receiver.MoveDirection
            .Where(x => x != Vector2.zero)
            .Subscribe(x => {
                Direction8 direction = DirectionMethods.FromVector(x);
                character.Move(direction);
            })
            .AddTo(this);
    }
}