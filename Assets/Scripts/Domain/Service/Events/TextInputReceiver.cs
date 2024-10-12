#nullable enable
using Cysharp.Threading.Tasks;
using R3;
using Unity.Logging;

namespace Domain.Service.Events
{
    public class TextInputReceiver
    {
        private readonly Subject<Unit> _onShownTextInput = new();
        public Observable<Unit> OnShownTextInput => _onShownTextInput;
        private readonly AsyncReactiveProperty<string> _onReceivedTextInput = new("");

        public async UniTask<string> GetTextInput()
        {
            Log.Debug("GetTextInput");
            ShowTextInput();
            var text = await _onReceivedTextInput.WaitAsync();
            Log.Debug($"GetTextInput: {text}");
            return text;
        }

        private void ShowTextInput()
        {
            _onShownTextInput.OnNext(Unit.Default);
        }

        public void SetTextInput(string text)
        {
            _onReceivedTextInput.Value = text;
        }
    }
}