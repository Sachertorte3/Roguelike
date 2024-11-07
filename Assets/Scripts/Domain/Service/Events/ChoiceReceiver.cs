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
            Log.Debug($"[Menu]GetChoice: {text} {string.Join(", ", choices)}");
            ShowChoices(text, choices);
            var index = await _onReceivedChoicedIndex.WaitAsync();
            Log.Debug($"[Menu]GetChoice: {choices[index]} {index}");
            return index;
        }

        private void ShowChoices(string? text, string[] choices)
        {
            _onShownChoice.OnNext((text, choices));
        }

        public void SetChoicedIndex(int index)
        {
            _onReceivedChoicedIndex.Value = index;
        }
    }
}