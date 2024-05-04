#nullable enable
using R3;
using RandomDungeonWithBluePrint;
using Scripts.Model;
using Scripts.Model.Characters;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Map;
using Scripts.Model.Setting;
using Scripts.Utilities;
using Scripts.View;
using System.Collections.Generic;
using UI;
using Unity.Logging;
using Unity.Logging.Sinks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;
using Logger = Unity.Logging.Logger;

namespace Scripts.Provider
{
    public class Presenter : IPostInitializable
    {
        private readonly InputReceiver receiver;
        private readonly TileViewContriller tileView;
        private readonly FieldBluePrint bluePrint;
        private readonly CameraFollowTarget _camera;
        private Dictionary<Character, CharacterView> characterViewDict = new Dictionary<Character, CharacterView>();
        [Inject]
        public Presenter(InputReceiver receiver, TileViewContriller tileView, FieldBluePrint bluePrint, CameraFollowTarget camera)
        {
            this.receiver = receiver;
            this.tileView = tileView;
            this.bluePrint = bluePrint;
            this._camera = camera;
        }
        public void PostInitialize()
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
            });
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
            GameManager.World = world;
            EffectViewSpawner effectViewer = new EffectViewSpawner();

            characterManager.OnCharacterAdded.Subscribe(character =>
            {
                GameObject prefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion();
                CharacterView view = Object.Instantiate(prefab).GetComponent<CharacterView>();
                view.transform.position = (Vector3Int)character.Position.CurrentValue;
                character.OnMove.Subscribe(move => view.Move(move.destination, move.direction));
                character.OnUseSkill.Subscribe(useSkill => effectViewer.Spawn(useSkill.skill.Area.Get(useSkill.position, useSkill.direction), Settings.EffectDisplayTime.CurrentValue));
                Settings.MoveMilliseconds.Subscribe(value => view.MoveMilliseconds = value);
                characterViewDict[character] = view;
            });
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
                });
            receiver.OnAttackPerformed.Subscribe(_ =>
            {
                actionReceiver.SetAttackAction();
            });
            characterManager.SpawnPlayer(map.GetAllPassablePositions().GetAtRandom(), actionReceiver);
            characterManager.SpawnCharacter(map.GetAllPassablePositions().GetAtRandom());
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
}