#nullable enable
using BidirectionalMap;
using R3;
using Scripts.Model.Characters;
using Scripts.Model.Characters.Effect;
using Scripts.Model.Setting;
using Scripts.Utilities;
using Scripts.View;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scripts.Provider
{
    public class SynchronizedCharacterView
    {
        private BiMap<Character, CharacterView> characterViewDict = new();
        private EffectViewSpawner _effectViewSpawner;
        private InputReceiver _inputReceiver;
        private Func<HashSet<Vector2Int>> _getVisibleArea;
        private GameObject _characterViewPrefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion();
        public SynchronizedCharacterView(EffectViewSpawner effectViewSpawner, InputReceiver receiver, CharacterManager characterManager)
        {
            _effectViewSpawner = effectViewSpawner;
            _inputReceiver = receiver;
            _getVisibleArea = characterManager.Player.Area.Get;

            Add(characterManager.Player);

            characterManager.OnCharacterAdded.Subscribe(character =>
            {
                Add(character);
            });
            characterManager.OnCharacterRemoved.Subscribe(character =>
            {
                Remove(character);
            });
        }
        public void Add(Character character)
        {
            CharacterView characterView = GameObject.Instantiate<GameObject>(_characterViewPrefab).GetComponent<CharacterView>();
            EntityView entityView = characterView.GetComponent<EntityView>();
            ObjectsManager.RegisterComponent(characterView.GetComponent<SpriteView>());
            characterView.Construct(character.TypeName());
            entityView.Construct(_inputReceiver);
            characterView.GetComponent<OverrideSprite>().SetTexture(character.TypeName(), character.SubtypeName(), character.TypeName() == "Human");
            characterView.transform.position = (Vector3Int)character.CurrentPosition;
            character.Direction.Subscribe(direction => characterView.Turn(direction));
            character.OnMove.Subscribe(move => entityView.Move(move.destination, move.direction));
            character.OnUseSkill.Subscribe(useSkill => _effectViewSpawner.Spawn(useSkill.skill.GetArea(useSkill.position, useSkill.direction), Settings.EffectDisplayTime.Value));
            Settings.MoveMilliseconds.Subscribe(value => entityView.MoveMilliseconds = value);
            Settings.DashMilliseconds.Subscribe(value => entityView.DashMilliseconds = value);
            SpriteView view = characterView.GetComponent<SpriteView>();
            view.SetVisibility(_getVisibleArea().Contains(character.CurrentPosition));
            entityView.OnMoveFinished.Subscribe(_ =>
            {
                view.SetVisibility(_getVisibleArea().Contains(character.CurrentPosition));
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