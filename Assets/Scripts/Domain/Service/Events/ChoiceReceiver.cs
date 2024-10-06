#nullable enable
using Cysharp.Threading.Tasks;
using R3;
using Unity.Logging;

namespace Domain.Service.Events
{
    public class ChoiceReceiver
    {
        private readonly Subject<(string? text, string[] choices)> _onShownChoice = new();
        public Observable<(string? text, string[] choices)> OnShownChoice => _onShownChoice;
        private readonly AsyncReactiveProperty<int> _onReceivedChoicedIndex = new(-1);

        public async UniTask<int> GetChoice(string? text, params string[] choices)
        {
            Log.Debug($"GetChoice: {text} {string.Join(", ", choices)}");
            SetChoices(text, choices);
            var index = await _onReceivedChoicedIndex.WaitAsync();
            Log.Debug($"GetChoice: {choices[index]} {index}");
            return index;
        }

        internal void SetChoices(string? text, string[] choices)
        {
            _onShownChoice.OnNext((text, choices));
        }

        public void SetChoicedIndex(int index)
        {
            _onReceivedChoicedIndex.Value = index;
        }
    }
}