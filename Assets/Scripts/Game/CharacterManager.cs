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
using Domain.Service.Rooms;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;

namespace Game
{
    public sealed class CharacterManager : IDisposable
    {
        private readonly ObservableList<ICharacter> _characters = new();
        private readonly CharacterFactory _factory = new();
        private HashSet<Vector2Int> _allCharacterPositions = new();

        public CharacterManager(CharacterMemento playerData, CharacterControlInputReceiver receiver, IMap map)
        {
            _characters.ObserveCountChanged().Subscribe(_ => SetAllCharacterPosition());
            _characters.SubscribeToAllObservables(
                character => character.Entity.Position,
                (character, _) => SetAllCharacterPosition()
            );
            _characters.SubscribeToAllObservables(
                character => character.Entity.OnDestroyed,
                (character, _) => _characters.Remove(character)
            );

            var player = _factory.CreatePlayer(playerData, receiver, map);

            Player = player;
            if (player.CurrentHp > 0)
            {
                AddCharacter(player);
            }
        }

        public readonly ICharacter Player;

        public IObservableCollection<ICharacter> Characters => _characters;

        public void Dispose()
        {
            _characters.ForEach(character => character.Dispose());
        }

        ~CharacterManager()
        {
            Dispose();
        }

        public ICharacter AddCharacter(ICharacter character)
        {
            _characters.Add(character);
            return character;
        }

        public void RemoveCharacter(ICharacter character)
        {
            _characters.Remove(character);
        }

        public ICharacter SpawnCharacter(CharacterMemento data, IMap map)
        {
            return AddCharacter(_factory.CreateCharacter(data, new EnemyBehavior(data.Behavior, map.Location), map));
        }

        public Ally SpawnAlly(CharacterMemento data, IMap map)
        {
            var behavior = new EnemyBehavior(data.Behavior, map.Location);
            return new Ally(
                AddCharacter(_factory.CreateCharacter(data, behavior, map)),
                behavior, map);
        }

        public HashSet<Vector2Int> GetAllCharacterPositions()
        {
            return _allCharacterPositions;
        }

        private void SetAllCharacterPosition()
        {
            _allCharacterPositions = _characters.Positions().ToHashSet();
        }
    }
}