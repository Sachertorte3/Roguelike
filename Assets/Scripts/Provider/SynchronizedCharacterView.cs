#nullable enable
using Model;
using R3;
using System.Collections.Generic;
using System.Linq;
using Data.Setting;
using Model.Domain.Characters;
using Model.Game;
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
        private readonly IReadOnlyCollection<Vector2Int> _visibleArea;

        [Inject]
        public SynchronizedCharacterView(EffectViewSpawner effectViewSpawner, InputReceiver receiver, World world)
        {
            _effectViewSpawner = effectViewSpawner;
            _inputReceiver = receiver;
            _visibleArea = world.VisibleArea;

            world.Characters.Set.SubscribeToAll(Add, Remove);
        }

        protected override CharacterView _viewPrefab => Addressables
            .LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion()
            .GetComponent<CharacterView>();

        protected override void InitializeView(Character character, CharacterView characterView)
        {
            characterView.Construct(character.TypeName());
            characterView.GetComponent<OverrideSprite>().SetTexture(character.TypeName(), character.SubtypeName(),
                character.TypeName() == "Human");
            characterView.transform.position = (Vector3Int)character.CurrentPosition;
            character.Direction.Subscribe(direction => characterView.Turn(direction));

            var entityView = characterView.GetComponent<EntityView>();
            entityView.Construct(_inputReceiver);
            character.OnMove.Subscribe(move => entityView.Move(move.destination, move.direction));
            character.OnTeleport.Subscribe(teleport => entityView.Teleport(teleport));
            character.OnSpawnEffect.Subscribe(useSkill =>
                _effectViewSpawner.Spawn(useSkill.Intersect(_visibleArea), Settings.EffectDisplayTime.Value));
            Settings.MoveMilliseconds.Subscribe(value => entityView.MoveMilliseconds = value);
            Settings.DashMilliseconds.Subscribe(value => entityView.DashMilliseconds = value);

            var spriteView = characterView.GetComponent<SpriteView>();
            spriteView.RegisterComponent();
            character.Visibility.Subscribe(visibility => spriteView.SetVisibility(visibility));

            var particleController = characterView.GetComponent<ParticleController>();
            character.StatusManager.Conditions.SubscribeToAll(
                conditionAdded => particleController.Add(conditionAdded.ParticleType),
                conditionRemoved => particleController.Remove(conditionRemoved.ParticleType)
            );
        }

        protected override void CleanupView(Character character, CharacterView characterView)
        {
        }
    }
}