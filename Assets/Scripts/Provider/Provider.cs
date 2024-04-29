using RandomDungeonWithBluePrint;
using Scripts.Model;
using Scripts.Model.Characters;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Map;
using Scripts.Utilities;
using Scripts.View;
using UniRx;
using Unity.Logging;
using Unity.Logging.Sinks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Logger = Unity.Logging.Logger;

public class Provider : MonoBehaviour
{
    [SerializeField] private InputReceiver receiver;
    [SerializeField] private TileViewContriller tileView;
    [SerializeField] private FieldBluePrint bluePrint;
    public void Start()
    {
        LoggerInit();

        Map map = new Map(bluePrint);
        map.OnChangeTile.Subscribe(context =>
        {
            switch (context.tile.TileType)
            {
                case TileCategory.Wall:
                    tileView.SetWall(context.position);
                    break;
                case TileCategory.Floor:
                    tileView.SetFloor(context.position);
                    break;
            }
        }).AddTo(this);
        foreach ((Vector2Int position, TileData tileData) in map.GetAllTiles())
        {
            switch (tileData.TileType)
            {
                case TileCategory.Wall:
                    tileView.SetWall(position);
                    break;
                case TileCategory.Floor:
                    tileView.SetFloor(position);
                    break;
            }
        }

        CharacterManager characterManager = new CharacterManager();
        characterManager.Characters.ObserveAdd().Subscribe(character =>
        {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion();
            CharacterView view = Instantiate(prefab).GetComponent<CharacterView>();
            view.transform.position = (Vector3Int)character.Value.Position.Value;
            character.Value.OnMove.Subscribe(direction => view.Move(direction)).AddTo(view);
        }).AddTo(this);
        ActionReceiver actionReceiver = new ActionReceiver();
        receiver.MoveDirection
            .Where(x => x != Vector2.zero)
            .Subscribe(x =>
            {
                Direction8 direction = DirectionMethods.FromVector(x);
                actionReceiver.SetMoveAction(direction);
            })
            .AddTo(this);
        characterManager.SpawnPlayer(map.GetAllPassablePositions().GetAtRandom(), actionReceiver);
        characterManager.SpawnCharacter(map.GetAllPassablePositions().GetAtRandom());
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