#nullable enable
using BidirectionalMap;
using R3;
using Scripts.Model;
using Scripts.Model.Characters;
using Scripts.Model.Setting;
using Scripts.Utilities;
using Scripts.View;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scripts.Provider
{
    public class SynchronizedCharacterView
    {
        private BiMap<Character, CharacterView> characterViewDict = new BiMap<Character, CharacterView>();
        private EffectViewSpawner _effectViewSpawner;
        private InputReceiver _inputReceiver;
        private VisibleArea _visibleArea;
        public SynchronizedCharacterView(EffectViewSpawner effectViewSpawner, InputReceiver receiver, VisibleArea area, CharacterManager characterManager)
        {
            _effectViewSpawner = effectViewSpawner;
            _inputReceiver = receiver;
            _visibleArea = area;

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
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion();
            CharacterView characterView = GameObject.Instantiate<GameObject>(prefab).GetComponent<CharacterView>();
            ObjectsManager.RegisterComponent<SpriteView>(characterView.GetComponent<SpriteView>());
            characterView.Construct(_inputReceiver, character.CharacterType.TypeName());
            characterView.GetComponent<OverrideSprite>().SetTexture(character.CharacterType.TypeName(), character.CharacterType.SubtypeName());
            characterView.transform.position = (Vector3Int)character.Position.CurrentValue;
            character.Direction.Subscribe(direction => characterView.Turn(direction));
            character.OnMove.Subscribe(move => characterView.Move(move.destination, move.direction));
            character.OnUseSkill.Subscribe<(Model.Characters.Effect.Skill skill, Vector2Int position, Direction8 direction)>(useSkill => _effectViewSpawner.Spawn(useSkill.skill.Area.Get(useSkill.position, useSkill.direction), Settings.EffectDisplayTime.Value));
            Settings.MoveMilliseconds.Subscribe(value => characterView.MoveMilliseconds = value);
            Settings.DashMilliseconds.Subscribe(value => characterView.DashMilliseconds = value);
            SpriteView view = characterView.GetComponent<SpriteView>();
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