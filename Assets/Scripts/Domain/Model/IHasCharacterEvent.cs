#nullable enable
namespace Domain.Model
{
    public interface IHasCharacterEvent
    {
        public ICharacterEvent Event { get; }
    }
}