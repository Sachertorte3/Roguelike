#nullable enable
using R3;

namespace Domain.Service.Events
{
    public class InfoReceiver
    {
        private readonly Subject<(string title, string info)> _onShownInfo = new();
        public Observable<(string title, string info)> OnShownInfo => _onShownInfo;
        public void ShowInfo(string title, string info)
        {
            _onShownInfo.OnNext((title, info));
        }
    }
}