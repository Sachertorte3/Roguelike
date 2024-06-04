using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Model.Domain;
using Model.Domain.Characters;
using Unity.Logging;
using UnityEngine;

namespace Model.Game
{
    public sealed class TurnController
    {
        private readonly GameInput _input;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning = false;
        private UniTaskCompletionSource _runCompletionSource;
        private int _turn = 1;

        public TurnController(GameInput input)
        {
            _input = input;
        }

        public async void Run(IMap map)
        {
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _runCompletionSource = new UniTaskCompletionSource();

            while (!_cancellationTokenSource.Token.IsCancellationRequested && map.Characters.Any())
            {
                Log.Debug($"Start turn {_turn}");
                foreach (var character in map.Characters.ToList())
                {
                    character.UpdateTurn(map);
                    if (character.CanAct && !character.StatusManager.IsDead)
                    {
                        await character.DoNextAction(map, _input);
                    }

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        _isRunning = false;
                        _runCompletionSource.TrySetResult();
                        break;
                    }
                }

                await map.Characters.Select(character =>
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