#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters;
using Domain.Service.Characters.Behavior;
using ObservableCollections;
using R3;
using UnityEngine;

namespace Game
{
    public sealed class CharacterManager : IDisposable
    {
        private readonly ObservableList<ICharacter> _characters = new();
        private readonly CharacterFactory _factory = new();
        public readonly CharacterEvents CharacterEvents = new();
        public readonly CharacterEvents PlayerEvents = new();
        private HashSet<Vector2Int> _allCharacterPositions = new();

        public CharacterManager(CharacterMemento playerData, CharacterControlInputReceiver receiver, IMap map)
        {
            _characters.ObserveCountChanged().Subscribe(_ => SetAllCharacterPosition());
            CharacterEvents.OnPositionChanged.Subscribe(_ => SetAllCharacterPosition());
            CharacterEvents.OnDestroyed.Subscribe(dead => _characters.Remove(dead.Character));

            var player = _factory.CreatePlayer(playerData, receiver, new ReactiveProperty<bool>(false), map);
            if (Player != null)
            {
                PlayerEvents.Remove(Player);
            }

            Player = player;
            if (player.CurrentHp > 0)
            {
                AddCharacter(player);
                PlayerEvents.Add(player);
            }
        }

        public readonly ICharacter Player;

        public IObservableCollection<ICharacter> Characters => _characters;

        public void Dispose()
        {
            _characters.ForEach(character => character.Dispose());
            PlayerEvents.Dispose();
            CharacterEvents.Dispose();
        }

        ~CharacterManager()
        {
            Dispose();
        }

        public ICharacter AddCharacter(ICharacter character)
        {
            _characters.Add(character);
            CharacterEvents.Add(character);
            return character;
        }

        public void RemoveCharacter(ICharacter character)
        {
            _characters.Remove(character);
            CharacterEvents.Remove(character);
        }

        public ICharacter SpawnCharacter(CharacterMemento data, IMap map)
        {
            return AddCharacter(_factory.CreateCharacter(data, new EnemyBehavior(data.Behavior, data.HomePosition), new ReactiveProperty<bool>(false), map));
        }

        public HashSet<Vector2Int> GetAllCharacterPositions()
        {
            return _allCharacterPositions;
        }

        private void SetAllCharacterPosition()
        {
            _allCharacterPositions = _characters.Select(character => character.CurrentPosition).ToHashSet();
        }
    }
}