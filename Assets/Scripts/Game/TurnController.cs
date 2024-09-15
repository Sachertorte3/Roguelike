using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Map;
using R3;
using Stats;
using Unity.Logging;
using UnityEngine;

namespace Game
{
    public sealed class TurnController
    {
        private readonly GameInput _input;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning = false;
        private UniTaskCompletionSource _runCompletionSource;
        private ReactiveProperty<int> _turn = new(1);
        private int _turnInLevel = 1;
        private Resource _turnWaitTime { get; init; }
        public ReadOnlyReactiveProperty<int> Turn => _turn;

        public TurnController(GameInput input)
        {
            _input = input;
            _turnWaitTime = new Resource(1);
        }

        public async void Run(IGameManager gameManager, IMap map)
        {
            if (_isRunning)
                throw new Exception("Turn is already running");
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _runCompletionSource = new UniTaskCompletionSource();

            while (!_cancellationTokenSource.Token.IsCancellationRequested && map.Characters.Any())
            {
                Log.Debug($"[Turn] Start turn {_turn}(in level:{_turnInLevel})\nCharacters:{map.Characters.Count}");

                map.UpdateTurn(_turn.CurrentValue);
                var characters = map.Characters.ToList();
                if (characters.Any(character => character.StatusManager.IsOverDrive))
                {
                    characters.RemoveAll(character => !character.StatusManager.IsOverDrive);
                }
                var minWaitTime = characters.Min(character => character.StatusManager.Stats.CurrentMaxWaitTime - character.StatusManager.Stats.CurrentWaitTime);

                if (_turnWaitTime.IsFull())
                {
                    _turnWaitTime.Set(0);
                }
                _turnWaitTime.Gain(minWaitTime);

                foreach (var character in characters)
                {
                    if (characters.Any(character => character.StatusManager.IsOverDrive) && !character.StatusManager.IsOverDrive)
                        continue;

                    if (_turnWaitTime.IsFull())
                    {
                        character.UpdateTurn();
                    }

                    character.StatusManager.AddWaitTime(minWaitTime);
                    if (character.StatusManager.IsWaitTimeFull())
                    {
                        character.StatusManager.ResetWaitTime();

                        if (character.CanAct && !character.StatusManager.IsDead)
                        {
                            Log.Debug($"[Turn] {character.GetName(map.Player)} think...");
                            await character.DoNextAction(gameManager, map, _input);
                        }
                        else
                        {
                            Log.Debug($"[Turn] {character.GetName(map.Player)} cannot act.");
                        }
                    }

                    await UniTask.WaitWhile(() => map.IsEventExecuting);

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        _isRunning = false;
                        _runCompletionSource.TrySetResult();
                        Log.Debug($"[Turn] loop canceled.");
                        return;
                    }
                }

                Globals.GameManager.Save();

                await characters.Select(character =>
                    UniTask.WaitUntil(() => character.State == CharacterState.Wait));

                _turn.Value++;
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