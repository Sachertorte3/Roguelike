#nullable enable
namespace Domain.Model.Character
{
    public interface IPlayer
    {
        public ICharacter Character { get; }
    }
}