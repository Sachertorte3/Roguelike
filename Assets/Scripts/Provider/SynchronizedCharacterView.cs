#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Game;
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
        protected override InputReceiver _inputReceiver { get; init; }
        private readonly World _world;

        protected override EntityView GetEntityView(CharacterView view)
        {
            return view.GetComponent<EntityView>();
        }

        [Inject]
        public SynchronizedCharacterView(InputReceiver receiver, World world)
        {
            _inputReceiver = receiver;
            _world = world;

            world.ActiveMap.SubscribeToAllItemsIgnoreNull(
                map => _disposable.Disposable = map.CharacterManager.Characters.SubscribeToAllItems(Add, Remove),
                map => map.Characters.ForEach(character => Remove(character))
            );
        }

        protected override CharacterView ViewPrefab(ICharacter _)
        {
            return Addressables
                .LoadAssetAsync<GameObject>("Assets/Prefabs/Character.prefab").WaitForCompletion()
                .GetComponent<CharacterView>();
        }

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
            characterView.Construct(character.CharacterType.TypeName(), character.IsEnemy(player.Character),
                character.IsAlly(player.Character));
            if (character.IsBoss)
                characterView.SetScale(1.5f);

            character.Status.Stats.HpValue.SubscribeToAllItems(hp =>
                characterView.UpdateHpBar(character.Status.Stats.MaxHp.CurrentValue, hp)).AddTo(characterView);
            character.Status.Stats.MaxHp.SubscribeToAllItems(maxHp =>
                    characterView.UpdateHpBar(maxHp, character.Status.Stats.HpValue.CurrentValue))
                .AddTo(characterView);

            characterView.GetComponent<OverrideSprite>().SetTexture(
                character.CharacterType.TypeName(),
                character.CharacterType.SubtypeName(),
                character.CharacterType.TypeName() == "Human");

            character.Direction.Subscribe(direction => characterView.Turn(direction)).AddTo(characterView);

            character.Entity.OnMove
                .Where(move => !move.isThrown)
                .Subscribe(move => characterView.PlayWalkAnimation().Forget())
                .AddTo(characterView);
            character.OnAttacked
                .Subscribe(useSkill => characterView.PlayAttackAnimation())
                .AddTo(characterView);

            var particleController = characterView.GetComponent<ParticleController>();
            if (character.IsShiny)
                particleController.Add(ParticleType.ShinyStar);
            character.Status.Conditions.SubscribeToAllItems(
                conditionAdded => particleController.Add(conditionAdded.ParticleType),
                conditionRemoved => particleController.Remove(conditionRemoved.ParticleType)
            ).AddTo(particleController);
        }

        protected override void CleanupView(ICharacter character, CharacterView characterView)
        {
        }
    }
}