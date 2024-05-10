#nullable enable
using ObservableCollections;
using R3;
using Scripts.Data.Area;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Map;
using Scripts.Model.Setting;
using Scripts.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using VContainer;
using System.Linq;

namespace Scripts.Model.Characters
{
    public sealed class CharacterManager
    {
        public Character Player => _player;
        private Character _player;
        private ObservableList<Character> _characters = new ObservableList<Character>();
        public ReadOnlyCollection<Character> Characters => new ReadOnlyCollection<Character>(_characters);
        public Observable<Character> OnCharacterAdded => _characters.ObserveAdd().Select(character => character.Value);
        public Observable<Character> OnCharacterRemoved => _characters.ObserveRemove().Select(character => character.Value);
        private readonly CharacterFactory _factory = new CharacterFactory();
        [Inject]
        public CharacterManager(Tilemap tilemap, CharacterControllInputReceiver actionReceiver)
        {
            Observable.Merge(
                _characters.ObserveAdd().Select(character => character.Value),
                _characters.ObserveRemove().Select(character => character.Value)
            )
                .Subscribe(character =>
            {
                character.Position.Subscribe(_ => SetAllCharacterPosition());
            });

            _player = _factory.CreateCharacter(tilemap.GetAllPassablePositions().GetAtRandom(), new PlayerBehavior(actionReceiver), Settings.IgnoreWall);
            AddCharacter(_player);
            _player.Area.OnVisibleAreaChanged.Subscribe(areaChanged =>
            {
                foreach (var character in _characters)
                {
                    if (areaChanged.AreaExited.Contains(character.CurrentPosition))
                    {
                        character.VisibleByPlayer = false;
                    }
                    else if (areaChanged.AreaEntered.Contains(character.CurrentPosition))
                    {
                        character.VisibleByPlayer = true;
                    }
                }
            });
        }
        private void AddCharacter(Character character)
        {
            _characters.Add(character);
            character.OnDead.Subscribe(_ => _characters.Remove(character));
        }
        public void SpawnCharacter(Vector2Int spawnPosition)
        {
            AddCharacter(_factory.CreateCharacter(spawnPosition, new EnemyBehavior(), new ReactiveProperty<bool>(false)));
        }
        public HashSet<Vector2Int> GetAllCharacterPositions() => _allCharacterPositions;
        private HashSet<Vector2Int> _allCharacterPositions = new HashSet<Vector2Int>();
        private void SetAllCharacterPosition()
        {
            _allCharacterPositions = Characters.Select(character => character.Position.CurrentValue).ToHashSet();
        }
    }
}
