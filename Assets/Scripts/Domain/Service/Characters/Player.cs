#nullable enable
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Behavior;
using R3;
using Unity.Logging;

namespace Domain.Service.Characters
{
    internal sealed class Player : IPlayer
    {
        public ICharacter Character { get; init; }
        private readonly ReactiveProperty<int> _money;
        public ReadOnlyReactiveProperty<int> Money => _money;

        public Player(PlayerMemento data, CharacterControlInputReceiver receiver, IMap map)
        {
            Character = new Character(data.Character, new PlayerBehavior(receiver), map, true);
            _money = new ReactiveProperty<int>(data.Money);
        }

        public PlayerMemento Serialize()
        {
            return new PlayerMemento(Character.Serialize(), _money.Value);
        }

        public void AddMoney(int value)
        {
            Log.Debug($"{Character.GetName(this)}:AddMoney {_money}+={value}");
            _money.Value += value;
        }

        public void ReduceMoney(int value)
        {
            Log.Debug($"{Character.GetName(this)}:ReduceMoney {_money}-={value}");
            _money.Value -= value;
        }
    }
}