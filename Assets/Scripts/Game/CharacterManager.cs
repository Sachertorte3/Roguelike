#nullable enable
using System;
using Domain.Model;
using Domain.Model.Character;
using Domain.Service;
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

        public CharacterManager()
        {
            CharacterEvents.OnDestroyed.Subscribe(dead => _characters.Remove(dead.Character));
        }

        public ICharacter? Player { get; private set; }

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

        private void SetPlayer(ICharacter player)
        {
            if (Player != null)
            {
                PlayerEvents.Remove(Player);
            }

            Player = player;
            AddCharacter(player);
            PlayerEvents.Add(player);
        }

        public void AddCharacter(ICharacter character)
        {
            _characters.Add(character);
            CharacterEvents.Add(character);
        }

        public void RemoveCharacter(ICharacter character)
        {
            _characters.Remove(character);
            CharacterEvents.Remove(character);
        }

        public void SpawnCharacter(CharacterMemento data, IMap world)
        {
            AddCharacter(_factory.CreateCharacter(data, new EnemyBehavior(data.wanderAround), new ReactiveProperty<bool>(false), world));
        }

        internal void SpawnPlayer(CharacterMemento playerData, CharacterControllInputReceiver receiver, IMap world)
        {
            SetPlayer(_factory.CreatePlayer(playerData, receiver, new ReactiveProperty<bool>(false), world));
        }
    }
}