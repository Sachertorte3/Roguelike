#nullable enable
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Setting;
using Model.Game;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedCharacterView : SynchronizedEntityView<ICharacter, CharacterView>
    {
        private readonly SerialDisposable _disposable = new();
        private readonly EffectViewSpawner _effectViewSpawner;
        protected override InputReceiver _inputReceiver { get; init; }
        private readonly World _world;
        protected override EntityView GetEntityView(CharacterView view) => view.GetComponent<EntityView>();

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

        protected override void InitializeView(ICharacter character, CharacterView characterView)
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

            character.OnEffectSpawned.Subscribe(useSkill =>
                _effectViewSpawner.Spawn(useSkill.Area.Intersect(_world.ActiveMap.CurrentValue.VisibleArea),
                    useSkill.Color, Settings.EffectDisplayTime.Value)
            ).AddTo(characterView);

            var particleController = characterView.GetComponent<ParticleController>();
            if (character.IsShiney)
                particleController.Add(ParticleType.ShineyStar);
            character.StatusManager.Conditions.SubscribeToAll(
                conditionAdded => particleController.Add(conditionAdded.ParticleType),
                conditionRemoved => particleController.Remove(conditionRemoved.ParticleType)
            ).AddTo(particleController);
        }

        protected override void CleanupView(ICharacter character, CharacterView characterView)
        {
        }
    }
}