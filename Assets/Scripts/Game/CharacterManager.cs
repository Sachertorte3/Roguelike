#nullable enable
using System;
using Domain.Model;
using Domain.Model.Character;
using Domain.Service.Characters;
using Domain.Service.Characters.Behavior;
using ObservableCollections;
using R3;

namespace Model.Game
{
    public sealed class CharacterManager : IDisposable
    {
        private readonly ObservableList<ICharacter> _characters = new();
        private readonly CharacterFactory _factory = new();
        public readonly CharacterEvents CharacterEvents = new();
        public readonly CharacterEvents PlayerEvents = new();

        public CharacterManager(CharacterMemento playerData, CharacterControllInputReceiver receiver, IMap map)
        {
            CharacterEvents.OnDestroyed.Subscribe(dead => _characters.Remove(dead.Character));

            var player = _factory.CreatePlayer(playerData, receiver, new ReactiveProperty<bool>(false), map);
            if (Player != null)
            {
                PlayerEvents.Remove(Player);
            }

            Player = player;
            AddCharacter(player);
            PlayerEvents.Add(player);
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
            return AddCharacter(_factory.CreateCharacter(data, new EnemyBehavior(data.wanderAround), new ReactiveProperty<bool>(false), map));
        }
    }
}