using Scripts.Model;
using Scripts.Model.Characters;
using Scripts.Model.Characters.Behavior;
using Scripts.Utilities;
using Scripts.View;
using UniRx;
using UnityEngine.AddressableAssets;
using Unity.Logging;
using Unity.Logging.Sinks;
using UnityEngine;
using Logger = Unity.Logging.Logger;

public class Spawner: MonoBehaviour
{
    [SerializeField] InputReceiver receiver;
    public void Start()
    {
        LoggerInit();

        CharacterManager characterManager = new CharacterManager();
        characterManager.Characters.ObserveAdd().Subscribe(character =>
        {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion();
            GameObject view = Instantiate(prefab);
            character.Value.MoveSubject.Subscribe(direction => view.GetComponent<CharacterView>().Move(direction));
        }).AddTo(this);
        ActionReceiver actionReceiver = new ActionReceiver();
        receiver.MoveDirection
            .Where(x => x != Vector2.zero)
            .Subscribe(x => {
                Direction8 direction = DirectionMethods.FromVector(x);
                actionReceiver.SetMoveAction(direction);
            })
            .AddTo(this);
        characterManager.SpawnPlayer(actionReceiver);
        characterManager.SpawnCharacter();
        new TurnController(characterManager);
    }
    private void LoggerInit()
    {
        Log.Logger = new Logger(new LoggerConfig()
            .MinimumLevel.Debug()
            .OutputTemplate("[{Timestamp}] {Level} | {Message}{NewLine}{Stacktrace}")
            .WriteTo.UnityDebugLog());
        Log.Debug("Init Logger");
    }
}