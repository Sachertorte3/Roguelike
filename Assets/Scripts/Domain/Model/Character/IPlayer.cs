#nullable enable
using Domain.Model.Memento;

namespace Domain.Model.Character
{
    public interface IPlayer : ISerializable<PlayerMemento>
    {
        public ICharacter Character { get; }
        public int Money { get; }
        public void AddMoney(int value);
        public void ReduceMoney(int value);
    }
}