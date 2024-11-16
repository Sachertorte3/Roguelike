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

        public CharacterManager(PlayerMemento playerData, CharacterControlInputReceiver receiver, IMap map)
        {
            _characters.ObserveCountChanged().Subscribe(_ => SetAllCharacterPosition());
            _characters.SubscribeToAllObservables(
                character => character.Entity.Position,
                (character, _) => SetAllCharacterPosition()
            );
            _characters.SubscribeToAllObservables(
                character => character.Entity.OnDestroyed,
                (character, _) => RemoveCharacter(character)
            );

            Player = _factory.CreatePlayer(playerData, receiver, map);

            if (Player.Character.CurrentHp > 0)
            {
                AddCharacter(Player.Character);
            }
        }

        public readonly IPlayer Player;

        public IObservableCollection<ICharacter> Characters => _characters;

        public void Dispose()
        {
            _characters.ForEach(character => character.Dispose());
        }

        ~CharacterManager()
        {
            Dispose();
        }

        public void AddCharacter(ICharacter character)
        {
            _characters.Add(character);
        }

        public void RemoveCharacter(ICharacter character)
        {
            _characters.Remove(character);
        }

        public ICharacter SpawnCharacter(CharacterMemento data, IMap map)
        {
            var character = _factory.CreateCharacter(data, new EnemyBehavior(data.Behavior, map.Location), map);
            AddCharacter(character);
            return character;
        }

        public Ally SpawnAlly(CharacterMemento data, IMap map)
        {
            var behavior = new EnemyBehavior(data.Behavior, map.Location);
            var character = _factory.CreateCharacter(data, behavior, map);
            AddCharacter(character);
            return new Ally(character, behavior, map);
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