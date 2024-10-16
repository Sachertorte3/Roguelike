#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Model
{
    public interface IHasEvent
    {
        public IEvent Event { get; }
    }

    public interface IEvent
    {
        public bool IsPlayerOnly { get; }
        public UniTask<bool> DoEvent(ICharacter character, IGameManager gameManager, IMap map);
    }
}