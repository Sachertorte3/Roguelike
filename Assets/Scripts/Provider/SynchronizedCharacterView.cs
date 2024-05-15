#nullable enable
using System;
using System.Collections.Generic;
using Assets.Scripts.View;
using BidirectionalMap;
using Model.Characters;
using Model.Setting;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities.ObjectsManager;
using View;
using Object = UnityEngine.Object;

namespace Provider
{
    public class SynchronizedCharacterView
    {
        private readonly GameObject _characterViewPrefab = Addressables
            .LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion();

        private readonly EffectViewSpawner _effectViewSpawner;
        private readonly InputReceiver _inputReceiver;
        private readonly BiMap<Character, CharacterView> characterViewDict = new();

        public SynchronizedCharacterView(EffectViewSpawner effectViewSpawner, InputReceiver receiver,
            CharacterManager characterManager)
        {
            _effectViewSpawner = effectViewSpawner;
            _inputReceiver = receiver;

            Add(characterManager.Player);

            characterManager.OnCharacterAdded.Subscribe(character => { Add(character); });
            characterManager.OnCharacterRemoved.Subscribe(character => { Remove(character); });
        }

        public void Add(Character character)
        {
            var characterView = Object.Instantiate(_characterViewPrefab).GetComponent<CharacterView>();
            characterView.Construct(character.TypeName());
            characterView.GetComponent<OverrideSprite>().SetTexture(character.TypeName(), character.SubtypeName(),
                character.TypeName() == "Human");
            characterView.transform.position = (Vector3Int)character.CurrentPosition;
            character.Direction.Subscribe(direction => characterView.Turn(direction));

            var entityView = characterView.GetComponent<EntityView>();
            entityView.Construct(_inputReceiver);
            character.OnMove.Subscribe(move => entityView.Move(move.destination, move.direction));
            character.OnSpawnEffect.Subscribe(useSkill =>
                _effectViewSpawner.Spawn(useSkill, Settings.EffectDisplayTime.Value));
            Settings.MoveMilliseconds.Subscribe(value => entityView.MoveMilliseconds = value);
            Settings.DashMilliseconds.Subscribe(value => entityView.DashMilliseconds = value);

            var spriteView = characterView.GetComponent<SpriteView>();
            spriteView.RegisterComponent();
            character.Visibility.Subscribe(visibility => spriteView.SetVisibility(visibility));
            characterViewDict.Add(character, characterView);

            var particleController = characterView.GetComponent<ParticleController>();
            character.Condition.OnConditionAdded.Subscribe(conditionAdded => particleController.Add(conditionAdded.ParticleType));
            character.Condition.OnConditionRemoved.Subscribe(conditionAdded => particleController.Remove(conditionAdded.ParticleType));
        }

        public void Remove(Character character)
        {
            Object.Destroy(characterViewDict.Forward[character].gameObject);
            characterViewDict.Remove(character);
        }

        public Character Get(CharacterView characterView)
        {
            return characterViewDict.Reverse[characterView];
        }

        public CharacterView Get(Character character)
        {
            return characterViewDict.Forward[character];
        }
    }
}