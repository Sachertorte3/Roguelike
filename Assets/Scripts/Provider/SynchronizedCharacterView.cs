#nullable enable
using System;
using System.Linq;
using Domain.Model.Setting;
using Domain.Service.Characters;
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
        private readonly SerialDisposable _disposable = new();
        private readonly EffectViewSpawner _effectViewSpawner;
        private readonly InputReceiver _inputReceiver;
        private readonly World _world;

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

        protected override CharacterView _viewPrefab => Addressables
            .LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion()
            .GetComponent<CharacterView>();

        ~SynchronizedCharacterView()
        {
            Dispose();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        protected override void InitializeView(Character character, CharacterView characterView)
        {
            var disposables = new CompositeDisposable();
            var player = _world.ActiveMap.CurrentValue.Player;
            characterView.Construct(character.CharacterType.TypeName(), character.IsEnemy(player),
                character.IsAlly(player));
            if (character.IsBoss)
                characterView.SetScale(1.5f);
            character.StatusManager.Stats.HpValue.SubscribeToAll(hp =>
                characterView.UpdateHpBar(character.StatusManager.Stats.MaxHp.CurrentValue, hp)).AddTo(characterView);
            character.StatusManager.Stats.MaxHp.SubscribeToAll(maxHp =>
                characterView.UpdateHpBar(maxHp, character.StatusManager.Stats.HpValue.CurrentValue)).AddTo(characterView);
            characterView.GetComponent<OverrideSprite>().SetTexture(character.CharacterType.TypeName(),
                character.CharacterType.SubtypeName(),
                character.CharacterType.TypeName() == "Human");
            characterView.transform.position = (Vector3Int)character.CurrentPosition;
            character.Direction.Subscribe(direction => characterView.Turn(direction)).AddTo(characterView);

            var entityView = characterView.GetComponent<EntityView>();
            entityView.Construct(_inputReceiver);
            character.OnMove.Subscribe(move => entityView.Move(move.destination, move.direction)).AddTo(entityView);
            character.OnTeleport.Subscribe(teleport => entityView.Teleport(teleport)).AddTo(entityView);
            character.OnEffectSpawned.Subscribe(useSkill =>
                _effectViewSpawner.Spawn(useSkill.Area.Intersect(_world.ActiveMap.CurrentValue.VisibleArea),
                    useSkill.Color, Settings.EffectDisplayTime.Value)
            ).AddTo(characterView);
            Settings.MoveMilliseconds.Subscribe(value => entityView.SetMoveMilliseconds(value)).AddTo(entityView);
            Settings.DashMilliseconds.Subscribe(value => entityView.SetDashMilliseconds(value)).AddTo(entityView);

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