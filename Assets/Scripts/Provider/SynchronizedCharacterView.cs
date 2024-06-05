#nullable enable
using System.Linq;
using Data.Setting;
using Model.Domain.Characters;
using Model.Game;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using Utilities.ObjectsManager;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedCharacterView : SynchronizedView<Character, CharacterView>
    {
        private readonly EffectViewSpawner _effectViewSpawner;
        private readonly InputReceiver _inputReceiver;
        private readonly World _world;
        private readonly SerialDisposable _disposable = new();

        [Inject]
        public SynchronizedCharacterView(EffectViewSpawner effectViewSpawner, InputReceiver receiver, World world)
        {
            _effectViewSpawner = effectViewSpawner;
            _inputReceiver = receiver;
            _world = world;

            world.ActiveMap.SubscribeToAllIgnoreNull(
                map => _disposable.Disposable = map.CharacterManager.Characters.SubscribeToAll(Add, Remove),
                map => map.Characters.ForEach(character => Remove(character))
            );
        }

        ~SynchronizedCharacterView()
        {
            Dispose();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        protected override CharacterView _viewPrefab => Addressables
            .LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion()
            .GetComponent<CharacterView>();

        protected override void InitializeView(Character character, CharacterView characterView)
        {
            var player = _world.ActiveMap.CurrentValue.Player;
            characterView.Construct(character.TypeName(), player.IsEnemy(character), player.IsAlly(character));
            characterView.GetComponent<OverrideSprite>().SetTexture(character.TypeName(), character.SubtypeName(),
                character.TypeName() == "Human");
            characterView.transform.position = (Vector3Int)character.CurrentPosition;
            character.Direction.Subscribe(direction => characterView.Turn(direction)).AddTo(characterView);

            var entityView = characterView.GetComponent<EntityView>();
            entityView.Construct(_inputReceiver);
            character.OnMove.Subscribe(move => entityView.Move(move.destination, move.direction)).AddTo(entityView);
            character.OnTeleport.Subscribe(teleport => entityView.Teleport(teleport)).AddTo(entityView);
            character.OnSpawnEffect.Subscribe(useSkill =>
                _effectViewSpawner.Spawn(useSkill.Intersect(_world.ActiveMap.CurrentValue.VisibleArea), Settings.EffectDisplayTime.Value)
            ).AddTo(characterView);
            Settings.MoveMilliseconds.Subscribe(value => entityView.MoveMilliseconds = value).AddTo(entityView);
            Settings.DashMilliseconds.Subscribe(value => entityView.DashMilliseconds = value).AddTo(entityView);

            var spriteView = characterView.GetComponent<SpriteView>();
            spriteView.RegisterComponent();
            character.Visibility.Subscribe(visibility => spriteView.SetVisibility(visibility)).AddTo(spriteView);

            var particleController = characterView.GetComponent<ParticleController>();
            character.StatusManager.Conditions.SubscribeToAll(
                conditionAdded => particleController.Add(conditionAdded.ParticleType),
                conditionRemoved => particleController.Remove(conditionRemoved.ParticleType)
            ).AddTo(particleController);
        }

        protected override void CleanupView(Character character, CharacterView characterView)
        {
        }
    }
}