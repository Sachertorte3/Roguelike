#nullable enable
using Model.Characters.Behavior;
using ObservableCollections;
using R3;
using System.Collections.Generic;
using UnityEngine;

namespace Model.Characters
{
    public sealed class CharacterManager
    {
        private readonly CharacterFactory _factory = new();
        public readonly CharacterEvents CharacterEvents = new();
        private readonly ObservableList<Character> _characters = new();

        public CharacterManager(HashSet<Vector2Int> visibleArea)
        {
            CharacterEvents.OnDead.Subscribe(dead => _characters.Remove(dead.Character));
        }

        internal IObservableCollection<Character> Characters => _characters;

        public void AddCharacter(Character character)
        {
            _characters.Add(character);
            CharacterEvents.Add(character);
        }

        public void RemoveCharacter(Character character)
        {
            _characters.Remove(character);
            CharacterEvents.Remove(character);
        }

        public void SpawnCharacter(Vector2Int spawnPosition)
        {
            AddCharacter(
                _factory.CreateCharacter(spawnPosition, new EnemyBehavior(), new ReactiveProperty<bool>(false)));
        }
    }
}