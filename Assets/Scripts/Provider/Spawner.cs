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
using Scripts.Model.Map;

public class Spawner: MonoBehaviour
{
    [SerializeField] InputReceiver receiver;
    [SerializeField] TileViewContriller tileView;
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
        Map map = new Map(10, 10);
        map.OnChangeTile.Subscribe(context =>
        {
            switch (context.tile.TileType)
            {
                case TileType.Wall:
                    tileView.SetWall(context.position);
                    break;
                case TileType.Floor:
                    tileView.SetFloor(context.position);
                    break;
            }
        });
        map.SetTest();
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