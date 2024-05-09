#nullable enable
using BidirectionalMap;
using R3;
using RandomDungeonWithBluePrint;
using Scripts.Model;
using Scripts.Model.Characters;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Map;
using Scripts.Model.Setting;
using Scripts.Utilities;
using Scripts.View;
using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
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
        [Inject]
        public Presenter(InputReceiver receiver, TileViewController tileView, TileMaskController tileMask, FieldBluePrint bluePrint, CameraFollowTarget camera, VisibleArea visibleArea)
        {
            LoggerInit();

            GameManager gameManager = new GameManager(bluePrint);

            CreateTilemap(gameManager.World.Map, tileView, tileMask);
            SetTilemapView(tileView, tileMask, gameManager.World.Map);

            EffectViewSpawner effectViewer = new EffectViewSpawner();

            SynchronizedCharacterView characters = new(effectViewer, receiver, visibleArea);

            gameManager.CharacterManager.OnCharacterAdded.Subscribe((character =>
            {
                characters.Add(character);
            }));
            gameManager.CharacterManager.OnCharacterRemoved.Subscribe(character =>
            {
                characters.Remove(character);
            });

            gameManager.Spawn(CreateActionReceiver(receiver));

            Globals.IsDash = () => receiver.IsDash;
            Globals.IsNoMove = () => receiver.IsNoMove;

            CharacterView playerView = characters.Get(gameManager.CharacterManager.Player);

            gameManager.CharacterManager.Player.Area.OnVisibleAreaChanged.Subscribe(area => visibleArea.UpdateArea(area));

            visibleArea.OnVisibleAreaChanged.Pairwise().Subscribe(area =>
            {
                area.Previous.ExceptWith(area.Current);
                area.Current.ExceptWith(area.Previous);
                tileMask.SetTilesTranslucent(area.Previous);
                tileMask.SetTilesVisible(area.Current);
                IEnumerable<Character> previousVisibleCharacter = gameManager.CharacterManager.Characters.Where(character => area.Previous.Contains(character.CurrentPosition));
                IEnumerable<Character> currentVisibleCharacter = gameManager.CharacterManager.Characters.Where(character => area.Current.Contains(character.CurrentPosition));
                previousVisibleCharacter.ForEach(character => character.VisibleByPlayer = false);
                currentVisibleCharacter.ForEach(character => character.VisibleByPlayer = true);
                ObjectsManager.GetObjectsByType<SpriteView>().Where(view => area.Previous.Contains(Vector2Int.RoundToInt(view.Position()))).ForEach(view => view.SetVisibility(false));
                ObjectsManager.GetObjectsByType<SpriteView>().Where(view => area.Current.Contains(Vector2Int.RoundToInt(view.Position()))).ForEach(view => view.SetVisibility(true));
            });
            ObjectsManager.ObserveAdd<SpriteView>().Subscribe(view => view.SetVisibility(visibleArea.Get().Contains(Vector2Int.RoundToInt(view.Position()))));

            gameManager.CharacterManager.Player.Area.Refrash(gameManager.CharacterManager.Player.CurrentPosition);

            GameObject arrowPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Arrow.prefab").WaitForCompletion();
            GameObject arrow = GameObject.Instantiate(arrowPrefab, playerView.transform);
            arrow.GetComponent<CharacterArrow>().Constract(playerView);

            camera.SetTarget(playerView.gameObject);

            gameManager.Run();
        }
        private void LoggerInit()
        {
            Log.Logger = new Logger(new LoggerConfig()
                .MinimumLevel.Debug()
                .OutputTemplate("[{Timestamp}] {Level} | {Message}{NewLine}{Stacktrace}")
                .WriteTo.UnityDebugLog());
            Log.Debug("Init Logger");
        }
        private CharacterControllInputReceiver CreateActionReceiver(InputReceiver receiver)
        {
            CharacterControllInputReceiver actionReceiver = new CharacterControllInputReceiver();
            receiver.OnMovePerformed
                .Where(vector => vector != Vector2.zero)
                .Subscribe(vector =>
                {
                    Direction8 direction = DirectionMethods.FromVector(vector);
                    actionReceiver.SetMoveInput(direction, true);
                });
            actionReceiver.OnActionRead.Select(_ => receiver.MoveVector)
                .Where(vector => vector != Vector2.zero)
                .Subscribe(vector =>
                {
                    Direction8 direction = DirectionMethods.FromVector(vector);
                    actionReceiver.SetMoveInput(direction, false);
                });
            receiver.OnAttackPerformed.Subscribe(_ =>
            {
                actionReceiver.SetAttackInput();
            });
            return actionReceiver;
        }
        private void CreateTilemap(ITilemapViewer map, TileViewController tileView, TileMaskController tileMask)
        {
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
                tileMask.ResetMask(context.position);
            });
        }
        private void SetTilemapView(TileViewController tileView, TileMaskController tileMask, ITilemapViewer map)
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
            tileMask.SetTilesTransparent(map.Rect.RectRange().ToHashSet());
        }
    }
    public class SynchronizedCharacterView
    {
        private BiMap<Character, CharacterView> characterViewDict = new BiMap<Character, CharacterView>();
        private EffectViewSpawner _effectViewSpawner;
        private InputReceiver _inputReceiver;
        private VisibleArea _visibleArea;
        public SynchronizedCharacterView(EffectViewSpawner effectViewSpawner, InputReceiver receiver, VisibleArea area)
        {
            _effectViewSpawner = effectViewSpawner;
            _inputReceiver = receiver;
            _visibleArea = area;
        }
        public void Add(Character character)
        {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion();
            CharacterView characterView = GameObject.Instantiate<GameObject>(prefab).GetComponent<CharacterView>();
            ObjectsManager.RegisterComponent<SpriteView>(characterView.GetComponent<SpriteView>());
            characterView.Construct(_inputReceiver);
            characterView.transform.position = (Vector3Int)character.Position.CurrentValue;
            character.Direction.Subscribe(direction => characterView.Turn(direction));
            character.OnMove.Subscribe(move => characterView.Move(move.destination, move.direction));
            character.OnUseSkill.Subscribe<(Model.Characters.Effect.Skill skill, Vector2Int position, Direction8 direction)>(useSkill => _effectViewSpawner.Spawn(useSkill.skill.Area.Get(useSkill.position, useSkill.direction), Settings.EffectDisplayTime.Value));
            Settings.MoveMilliseconds.Subscribe(value => characterView.MoveMilliseconds = value);
            Settings.DashMilliseconds.Subscribe(value => characterView.DashMilliseconds = value);
            SpriteView view = characterView.GetComponent<SpriteView>();
            Debug.Log(_visibleArea.Get());
            view.SetVisibility(_visibleArea.Get().Contains(character.CurrentPosition));
            characterView.OnMoveFinished.Subscribe(_ =>
            {
                view.SetVisibility(_visibleArea.Get().Contains(character.CurrentPosition));
            });
            characterViewDict.Add(character, characterView);
        }
        public void Remove(Character character)
        {
            GameObject.Destroy(characterViewDict.Forward[character].gameObject);
            characterViewDict.Remove(character);
        }
        public Character Get(CharacterView characterView) => characterViewDict.Reverse[characterView];
        public CharacterView Get(Character character) => characterViewDict.Forward[character];
    }
}