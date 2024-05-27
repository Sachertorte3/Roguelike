#nullable enable
using Data.Character;
using Model.Domain;
using Model.Domain.Characters;
using Model.Domain.Characters.Behavior;
using ObservableCollections;
using R3;
using UnityEngine;

namespace Model.Game
{
    public sealed class CharacterManager
    {
        private readonly ObservableList<Character> _characters = new();
        private readonly CharacterFactory _factory = new();
        public readonly CharacterEvents CharacterEvents = new();

        public CharacterManager()
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

        public void SpawnCharacter(EnemyData data, Vector2Int spawnPosition, IWorld world)
        {
            AddCharacter(
                _factory.CreateCharacter(data, spawnPosition, new EnemyBehavior(), new ReactiveProperty<bool>(false),
                    world));
        }
    }
}