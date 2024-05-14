#nullable enable
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Model.Characters.Behavior;
using Model.Map;
using Model.Setting;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;
using VContainer;

namespace Model.Characters
{
    public sealed class CharacterManager
    {
        private readonly CharacterFactory _factory = new();
        public readonly CharacterEvents CharacterEvents = new();
        public readonly CharacterEvents PlayerEvents = new();
        private HashSet<Vector2Int> _allCharacterPositions = new();
        private readonly ObservableList<Character> _characters = new();

        [Inject]
        public CharacterManager(Tilemap tilemap, CharacterControllInputReceiver actionReceiver)
        {
            _characters.ObserveCountChanged().Subscribe(_ => SetAllCharacterPosition());
            CharacterEvents.OnPositionChanged.Subscribe(positionChanged =>
            {
                SetAllCharacterPosition();
                positionChanged.Character.SetVisiblity(Player.Area.Get().Contains(positionChanged.Position));
            });
            CharacterEvents.OnDead.Subscribe(dead => _characters.Remove(dead.Character));

            Player = _factory.CreateCharacter(tilemap.GetAllPassablePositions().GetAtRandom(),
                new PlayerBehavior(actionReceiver), Settings.IgnoreWall);
            AddCharacter(Player);
            PlayerEvents.Add(Player);
            PlayerEvents.OnVisibleAreaChanged.Subscribe(areaChanged =>
            {
                foreach (var character in _characters)
                    if (areaChanged.AreaExited.Contains(character.CurrentPosition))
                        character.SetVisiblity(false);
                    else if (areaChanged.AreaEntered.Contains(character.CurrentPosition)) character.SetVisiblity(true);
            });
        }

        public Character Player { get; init; }

        public ReadOnlyCollection<Character> Characters => new(_characters);
        public Observable<Character> OnCharacterAdded => _characters.ObserveAdd().Select(character => character.Value);

        public Observable<Character> OnCharacterRemoved =>
            _characters.ObserveRemove().Select(character => character.Value);

        private void AddCharacter(Character character)
        {
            _characters.Add(character);
            CharacterEvents.Add(character);
        }

        public void SpawnCharacter(Vector2Int spawnPosition)
        {
            AddCharacter(
                _factory.CreateCharacter(spawnPosition, new EnemyBehavior(), new ReactiveProperty<bool>(false)));
        }

        public HashSet<Vector2Int> GetAllCharacterPositions()
        {
            return _allCharacterPositions;
        }

        private void SetAllCharacterPosition()
        {
            _allCharacterPositions = Characters.Select(character => character.Position.CurrentValue).ToHashSet();
        }
    }
}