#nullable enable
using Codice.Client.BaseCommands;
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
using Logger = Unity.Logging.Logger;

namespace Scripts.Provider
{
    public class Presenter
    {
        private Dictionary<Character, CharacterView> characterViewDict = new Dictionary<Character, CharacterView>();
        [Inject]
        public Presenter(InputReceiver receiver, TileViewContriller tileView, FieldBluePrint bluePrint, CameraFollowTarget camera)
        {
            LoggerInit();

            Tilemap map = CreateTilemap(bluePrint, tileView);
            SetTilemapView(tileView, map);

            EffectViewSpawner effectViewer = new EffectViewSpawner();

            CharacterManager characterManager = CreateCharacterManager(effectViewer, receiver);

            CreateWorld(map, characterManager);

            characterManager.SpawnPlayer(map.GetAllPassablePositions().GetAtRandom(), CreateActionReceiver(receiver));
            characterManager.SpawnCharacter(map.GetAllPassablePositions().GetAtRandom());

            GameManager.IsDash = () => receiver.IsDash;
            GameManager.IsNoMove = () => receiver.IsNoMove;

            camera.SetTarget(characterViewDict[characterManager.Player].gameObject);

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
        private ActionReceiver CreateActionReceiver(InputReceiver receiver)
        {
            ActionReceiver actionReceiver = new ActionReceiver();
            receiver.OnMovePerformed
                .Where(vector => vector != Vector2.zero)
                .Subscribe(vector =>
                {
                    Direction8 direction = DirectionMethods.FromVector(vector);
                    actionReceiver.SetMoveAction(direction, true);
                });
            actionReceiver.OnActionRead.Select(_ => receiver.MoveVector)
                .Where(vector => vector != Vector2.zero)
                .Subscribe(vector =>
                {
                    Direction8 direction = DirectionMethods.FromVector(vector);
                    actionReceiver.SetMoveAction(direction, false);
                });
            receiver.OnAttackPerformed.Subscribe(_ =>
            {
                actionReceiver.SetAttackAction();
            });
            return actionReceiver;
        }
        private Tilemap CreateTilemap(FieldBluePrint bluePrint, TileViewContriller tileView)
        {
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
            return map;
        }
        private void SetTilemapView(TileViewContriller tileView, Tilemap map)
        {
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
        }
        private CharacterManager CreateCharacterManager(EffectViewSpawner effectViewSpawner, InputReceiver receiver)
        {
            CharacterManager characterManager = new CharacterManager();

            characterManager.OnCharacterAdded.Subscribe(character =>
            {
                GameObject prefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion();
                CharacterView view = Object.Instantiate(prefab).GetComponent<CharacterView>();
                view.Construct(receiver);
                view.transform.position = (Vector3Int)character.Position.CurrentValue;
                character.OnMove.Subscribe(move => view.Move(move.destination, move.direction));
                character.OnUseSkill.Subscribe(useSkill => effectViewSpawner.Spawn(useSkill.skill.Area.Get(useSkill.position, useSkill.direction), Settings.EffectDisplayTime.Value));
                Settings.MoveMilliseconds.Subscribe(value => view.MoveMilliseconds = value);
                Settings.DashMilliseconds.Subscribe(value => view.DashMilliseconds = value);
                characterViewDict[character] = view;
            });
            characterManager.OnCharacterRemoved.Subscribe(character =>
            {
                GameObject.Destroy(characterViewDict[character].gameObject);
                characterViewDict.Remove(character);
            });

            return characterManager;
        }
        private void CreateWorld(Tilemap map, CharacterManager characterManager)
        {
            World world = new World(map, characterManager);
            GameManager.World = world;
        }
    }
}