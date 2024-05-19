#nullable enable
using Model.Characters.Behavior;
using ObservableCollections;
using R3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Model.Characters
{
    public sealed class CharacterManager
    {
        private readonly CharacterFactory _factory = new();
        public readonly CharacterEvents CharacterEvents = new();
        private HashSet<Vector2Int> _allCharacterPositions = new();
        private readonly ObservableList<Character> _characters = new();

        public CharacterManager(Character player)
        {
            _characters.ObserveCountChanged().Subscribe(_ => SetAllCharacterPosition());
            CharacterEvents.OnPositionChanged.Subscribe(positionChanged =>
            {
                SetAllCharacterPosition();
                positionChanged.Character.SetVisiblity(player.Area.Get().Contains(positionChanged.Position));
            });
            CharacterEvents.OnDead.Subscribe(dead => _characters.Remove(dead.Character));
            AddCharacter(player);
        }

        public IObservableCollection<Character> Characters => _characters;

        public void AddCharacter(Character character)
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
            return new HashSet<Vector2Int>(_allCharacterPositions);
        }

        private void SetAllCharacterPosition()
        {
            _allCharacterPositions = Characters.Select(character => character.Position.CurrentValue).ToHashSet();
        }
    }
}