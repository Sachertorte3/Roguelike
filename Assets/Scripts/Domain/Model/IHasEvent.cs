#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Map;

namespace Domain.Model
{
    public interface IHasEvent
    {
        public string? ChoiceMessage { get; }
        public string ChoiceText { get; }
        public bool CanBeCanceled { get; }
        public bool CanExecuteEvent { get; }
        public UniTask DoEvent(IGameManager gameManager, IMapManager mapManager);
    }
}