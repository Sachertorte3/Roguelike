using Scripts.Model;
using Scripts.Utilities;
using Scripts.View;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.PlayerLoop;

public class Spawner: MonoBehaviour
{
    [SerializeField] InputReceiver receiver;
    public void Start()
    {
        CharacterManager characterManager = new CharacterManager();
        characterManager.Characters.ObserveAdd().Subscribe(character =>
        {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion();
            GameObject view = Instantiate(prefab);
            character.Value.MoveSubject.Subscribe(direction => view.GetComponent<CharacterView>().Move(direction));
        }).AddTo(this);
        Character player = characterManager.SpawnPlayer();
        receiver.MoveDirection
            .Where(x => x != Vector2.zero)
            .Subscribe(x => {
                Direction8 direction = DirectionMethods.FromVector(x);
                player.Move(direction);
            })
            .AddTo(this);
        new TurnController(characterManager);
    }
}