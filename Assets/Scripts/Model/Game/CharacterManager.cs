#nullable enable
using System;
using Data.Character;
using Model.Domain;
using Model.Domain.Characters;
using Model.Domain.Characters.Behavior;
using ObservableCollections;
using R3;
using UnityEngine;

namespace Model.Game
{
    public sealed class CharacterManager : IDisposable
    {
        public Character? Player { get; private set;}
        private readonly ObservableList<Character> _characters = new();
        private readonly CharacterFactory _factory = new();
        public readonly CharacterEvents PlayerEvents = new();
        public readonly CharacterEvents CharacterEvents = new();

        public CharacterManager()
        {
            CharacterEvents.OnDead.Subscribe(dead => _characters.Remove(dead.Character));
        }
        ~CharacterManager()
        {
            Dispose();
        }

        public void Dispose()
        {
            _characters.ForEach(character => character.Dispose());
            PlayerEvents.Dispose();
            CharacterEvents.Dispose();
        }

        public IObservableCollection<Character> Characters => _characters;

        private void SetPlayer(Character player)
        {
            if (Player != null)
            {
                PlayerEvents.Remove(Player);
            }
            Player = player;
            AddCharacter(player);
            PlayerEvents.Add(player);
        }
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

        public void SpawnCharacter(EnemyData data, Vector2Int spawnPosition, IMap world)
        {
            AddCharacter(
                _factory.CreateCharacter(data, spawnPosition, new EnemyBehavior(), new ReactiveProperty<bool>(false),
                    world));
        }

        internal void SpawnPlayer(Vector2Int spawnPosition, CharacterControllInputReceiver receiver, IMap world)
        {
            SetPlayer(_factory.CreatePlayer(spawnPosition, receiver, new ReactiveProperty<bool>(false), world));
        }
    }
}