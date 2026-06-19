#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model;
using R3;

namespace Domain.Service.Events
{
    // チュートリアル表示の待受。ChoiceReceiver と同じく、表示を通知してから
    // 決定キー（Advance）が押されるまで待つ。ボタンは持たない。
    public class TutorialReceiver
    {
        private readonly Subject<TutorialType> _onShown = new();
        public Observable<TutorialType> OnShown => _onShown;
        private readonly Subject<Unit> _onHidden = new();
        public Observable<Unit> OnHidden => _onHidden;
        private readonly AsyncReactiveProperty<Unit> _onAdvanced = new(Unit.Default);

        // 指定種類のチュートリアルを表示し、決定キーが押される（Advance）まで待つ。
        public async UniTask Show(TutorialType type)
        {
            _onShown.OnNext(type);
            await _onAdvanced.WaitAsync();
            _onHidden.OnNext(Unit.Default);
        }

        public void Advance()
        {
            _onAdvanced.Value = Unit.Default;
        }
    }
}
