using BidirectionalMap;
using RandomDungeonWithBluePrint;
using Scripts.Model.Characters;
using Scripts.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI;
using VContainer;
using R3;
using UnityEngine.AddressableAssets;
using UnityEngine;
using Scripts.Utilities;
using Scripts.Model.Setting;
using Sirenix.Utilities;

namespace Scripts.Provider
{
    internal class CharacterPresenter
    {
        private BiMap<Character, CharacterView> characterViewDict = new BiMap<Character, CharacterView>();
        [Inject]
        public CharacterPresenter(CharacterManager characterManager, InputReceiver receiver, TileMaskController tileMask)
        {
            EffectViewSpawner effectViewer = new EffectViewSpawner();

            characterManager.OnCharacterAdded.Subscribe((character =>
            {
                GameObject prefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/CharacterView.prefab").WaitForCompletion();
                CharacterView characterView = GameObject.Instantiate<GameObject>(prefab).GetComponent<CharacterView>();
                characterView.GetComponent<SpriteView>().RegisterComponent();
                characterView.Construct(receiver);
                characterView.transform.position = (Vector3Int)character.Position.CurrentValue;
                character.OnMove.Subscribe(move => characterView.Move(move.destination, move.direction));
                character.OnUseSkill.Subscribe(useSkill => effectViewer.Spawn(useSkill.skill.Area.Get(useSkill.position, useSkill.direction), Settings.EffectDisplayTime.Value));
                Settings.MoveMilliseconds.Subscribe(value => characterView.MoveMilliseconds = value);
                Settings.DashMilliseconds.Subscribe(value => characterView.DashMilliseconds = value);
                characterViewDict.Add(character, characterView);
                SpriteView view = characterView.GetComponent<SpriteView>();
                view.SetVisibility(characterManager.Player.Area.Get().Contains(character.CurrentPosition));
                characterView.OnMoveFinished.Subscribe(_ =>
                {
                    view.SetVisibility(characterManager.Player.Area.Get().Contains(character.CurrentPosition));
                });
            }));
            characterManager.OnCharacterRemoved.Subscribe(character =>
            {
                GameObject.Destroy(characterViewDict.Forward[character].gameObject);
                characterViewDict.Remove(character);
            });

            characterManager.Player.Area.OnVisibleAreaChanged.Pairwise().Subscribe(area =>
            {
                area.Previous.ExceptWith(area.Current);
                area.Current.ExceptWith(area.Previous);
                tileMask.SetTilesTranslucent(area.Previous);
                tileMask.SetTilesVisible(area.Current);
                IEnumerable<Character> previousVisibleCharacter = characterManager.Characters.Where(character => area.Previous.Contains(character.CurrentPosition));
                IEnumerable<Character> currentVisibleCharacter = characterManager.Characters.Where(character => area.Current.Contains(character.CurrentPosition));
                previousVisibleCharacter.ForEach(character => character.VisibleByPlayer = false);
                currentVisibleCharacter.ForEach(character => character.VisibleByPlayer = true);
                ObjectsManager.GetObjectsByType<SpriteView>().Where(view => area.Previous.Contains(Vector2Int.RoundToInt(view.Position()))).ForEach(view => view.SetVisibility(false));
                ObjectsManager.GetObjectsByType<SpriteView>().Where(view => area.Current.Contains(Vector2Int.RoundToInt(view.Position()))).ForEach(view => view.SetVisibility(true));
            });
        }
    }
}
