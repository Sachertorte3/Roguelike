#nullable enable
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Behavior;
using Unity.Logging;

namespace Domain.Service.Characters
{
    internal sealed class Player : IPlayer
    {
        public ICharacter Character { get; init; }
        public int Money { get; private set; }

        public Player(PlayerMemento data, CharacterControlInputReceiver receiver, IMap map)
        {
            Character = new Character(data.Character, new PlayerBehavior(receiver), map, true);
            Money = data.Money;
        }

        public PlayerMemento Serialize()
        {
            return new PlayerMemento(Character.Serialize(), Money);
        }

        public void AddMoney(int value)
        {
            Log.Debug($"{Character.GetName(this)}:AddMoney {Money}+={value}");
            Money += value;
        }

        public void ReduceMoney(int value)
        {
            Log.Debug($"{Character.GetName(this)}:ReduceMoney {Money}-={value}");
            Money -= value;
        }
    }
}