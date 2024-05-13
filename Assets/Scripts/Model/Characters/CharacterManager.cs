#nullable enable
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ObservableCollections;
using R3;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Map;
using Scripts.Model.Setting;
using Scripts.Utilities;
using UnityEngine;
using VContainer;

namespace Scripts.Model.Characters
{
    public sealed class CharacterManager
    {
        public Character Player => _player;
        private Character _player;
        private ObservableList<Character> _characters = new();
        public ReadOnlyCollection<Character> Characters => new(_characters);
        public Observable<Character> OnCharacterAdded => _characters.ObserveAdd().Select(character => character.Value);
        public Observable<Character> OnCharacterRemoved => _characters.ObserveRemove().Select(character => character.Value);
        private readonly CharacterFactory _factory = new();
        public readonly CharacterEvents PlayerEvents = new();
        public readonly CharacterEvents CharacterEvents = new();
        [Inject]
        public CharacterManager(Tilemap tilemap, CharacterControllInputReceiver actionReceiver)
        {
            _characters.ObserveCountChanged().Subscribe(_ => SetAllCharacterPosition());
            CharacterEvents.OnPositionChanged.Subscribe(_ => SetAllCharacterPosition());
            CharacterEvents.OnDead.Subscribe(dead => _characters.Remove(dead.Character));

            _player = _factory.CreateCharacter(tilemap.GetAllPassablePositions().GetAtRandom(), new PlayerBehavior(actionReceiver), Settings.IgnoreWall);
            AddCharacter(_player);
            PlayerEvents.Add(_player);
            PlayerEvents.OnVisibleAreaChanged.Subscribe(areaChanged =>
            {
                foreach (var character in _characters)
                {
                    if (areaChanged.AreaExited.Contains(character.CurrentPosition))
                    {
                        character.SetVisiblity(false);
                    }
                    else if (areaChanged.AreaEntered.Contains(character.CurrentPosition))
                    {
                        character.SetVisiblity(true);
                    }
                }
            });
        }
        private void AddCharacter(Character character)
        {
            _characters.Add(character);
            CharacterEvents.Add(character);
        }
        public void SpawnCharacter(Vector2Int spawnPosition)
        {
            AddCharacter(_factory.CreateCharacter(spawnPosition, new EnemyBehavior(), new ReactiveProperty<bool>(false)));
        }
        public HashSet<Vector2Int> GetAllCharacterPositions() => _allCharacterPositions;
        private HashSet<Vector2Int> _allCharacterPositions = new();
        private void SetAllCharacterPosition()
        {
            _allCharacterPositions = Characters.Select(character => character.Position.CurrentValue).ToHashSet();
        }
    }
}
