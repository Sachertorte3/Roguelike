#nullable enable
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Behavior;

namespace Domain.Service.Characters
{
    internal sealed class Player : IPlayer
    {
        public ICharacter Character { get; init; }
        public Player(CharacterMemento data, CharacterControlInputReceiver receiver, IMap map)
        {
            Character = new Character(data, new PlayerBehavior(receiver), map, true);
        }
    }
}