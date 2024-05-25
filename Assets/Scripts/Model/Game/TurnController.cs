using Cysharp.Threading.Tasks;
using Model.Characters;
using Model.Game;
using Model.Logs;
using R3;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Logging;

namespace Model
{
    public sealed class TurnController
    {
        private readonly World _world;
        private readonly GameInput _input;
        private IEnumerable<Character> GetCharacters() => _world.Characters.Set;
        private int _turn = 1;
        private bool _isRunning = false;
        private CancellationTokenSource _cancellationTokenSource;
        private UniTaskCompletionSource _runCompletionSource;

        public TurnController(World world, GameInput input)
        {
            _world = world;
            _input = input;
        }

        public async void Run()
        {
            _isRunning = true;
            _cancellationTokenSource = new();
            _runCompletionSource = new();

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                Log.Debug($"Start turn {_turn}");
                GameLog.Add($"Start turn {_turn}");
                IEnumerable<Character> characterList = GetCharacters();
                foreach (var character in characterList.ToList())
                {
                    character.UpdateTurn();
                    if (character.CanAct && !character.StatusManager.IsDead)
                    {
                        character.State = CharacterState.Think;
                        await character.DoNextAction(_world, _input);
                    }
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        _isRunning = false;
                        _runCompletionSource.TrySetResult();
                        break;
                    }
                }
                await characterList.Select(character =>
                    UniTask.WaitUntil(() => character.State == CharacterState.Wait));
                _turn++;
            }
        }
        public async UniTask Stop()
        {
            if (!_isRunning) return;
            _cancellationTokenSource.Cancel();
            await _runCompletionSource.Task;
        }
    }
}