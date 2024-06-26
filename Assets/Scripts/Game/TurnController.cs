using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain.Service;
using Domain.Service.Characters;
using Unity.Logging;

namespace Model.Game
{
    public sealed class TurnController
    {
        private readonly GameInput _input;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning = false;
        private UniTaskCompletionSource _runCompletionSource;
        private int _turn = 1;
        private int _turnInLevel = 1;

        public TurnController(GameInput input)
        {
            _input = input;
        }

        public async void Run(IMap map)
        {
            if (_isRunning)
                throw new Exception("Turn is already running");
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _runCompletionSource = new UniTaskCompletionSource();

            while (!_cancellationTokenSource.Token.IsCancellationRequested && map.Characters.Any())
            {
                Log.Debug($"[Turn] Start turn {_turn}(in level:{_turnInLevel})\nCharacters:{map.Characters.Count}");
                var characters = map.Characters.ToList();
                foreach (var character in characters)
                {
                    character.UpdateTurn(map);
                    if (character.CanAct && !character.StatusManager.IsDead)
                    {
                        Log.Debug($"[Turn] {character.Name} think...");
                        await character.DoNextAction(map, _input);
                    }
                    else
                    {
                        Log.Debug($"[Turn] {character.Name} cannot act.");
                    }

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        _isRunning = false;
                        _runCompletionSource.TrySetResult();
                        Log.Debug($"[Turn] loop canceled.");
                        return;
                    }
                }

                await characters.Select(character =>
                    UniTask.WaitUntil(() => character.State == CharacterState.Wait));
                _turn++;
                _turnInLevel++;
            }
            _isRunning = false;
            _runCompletionSource.TrySetResult();
        }

        public async UniTask Stop()
        {
            if (!_isRunning) return;
            _turnInLevel = 1;
            _cancellationTokenSource.Cancel();
            await _runCompletionSource.Task;
            Log.Debug("[Turn] Stop");
        }
    }
}