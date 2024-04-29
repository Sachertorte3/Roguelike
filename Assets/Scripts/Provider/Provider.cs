#nullable enable
using RandomDungeonWithBluePrint;
using Scripts.Model;
using Scripts.Model.Characters;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Map;
using Scripts.Utilities;
using Scripts.View;
using System.Collections.Generic;
using UI;
using UniRx;
using UniRx.Triggers;
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
    [SerializeField] private CameraFollowTarget _camera;
    private Dictionary<Character, CharacterView> characterViewDict = new Dictionary<Character, CharacterView>();
    public void Start()
    {
        LoggerInit();

        Tilemap map = new Tilemap(bluePrint);
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

        World world = new World(map, characterManager);

        characterManager.Characters.ObserveAdd().Subscribe(character =>
        {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion();
            CharacterView view = Instantiate(prefab).GetComponent<CharacterView>();
            view.transform.position = (Vector3Int)character.Value.Position.Value;
            character.Value.OnMove.Subscribe(direction => view.Move(direction)).AddTo(view);
            characterViewDict[character.Value] = view;
        }).AddTo(this);
        ActionReceiver actionReceiver = new ActionReceiver();
        Observable.Merge(
            receiver.OnMovePerformed,
            actionReceiver.OnWait.Select(_ => receiver.MoveVector)
        )
            .Where(vector => vector != Vector2.zero)
            .Subscribe(vector =>
            {
                Direction8 direction = DirectionMethods.FromVector(vector);
                actionReceiver.SetMoveAction(direction);
            })
            .AddTo(this);
        characterManager.SpawnPlayer(map.GetAllPassablePositions().GetAtRandom(), actionReceiver, world);
        characterManager.SpawnCharacter(map.GetAllPassablePositions().GetAtRandom(), world);
        _camera.SetTarget(characterViewDict[characterManager.Player].gameObject);

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