using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Map;
using Domain.Model.Setting;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities.Stats;

namespace Game
{
    public sealed class TurnController
    {
        private readonly GameInput _input;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;
        private UniTaskCompletionSource _runCompletionSource;
        private ReactiveProperty<int> _turnInLevel = new(0);
        private Subject<Unit> _onTurnChanged = new();
        private Resource _turnWaitTime { get; init; }
        public ReadOnlyReactiveProperty<int> TurnInLevel => _turnInLevel;
        public Observable<Unit> OnTurnChanged => _onTurnChanged;

        public TurnController(GameInput input)
        {
            _input = input;
            _turnWaitTime = new Resource(1);
        }

        public async void Run(IGameManager gameManager, IMap map)
        {
            _turnInLevel.Value = 0;
            if (_isRunning)
                throw new Exception("Turn is already running");
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _runCompletionSource = new UniTaskCompletionSource();

            while (!_cancellationTokenSource.Token.IsCancellationRequested && map.Characters.Any())
            {
                var characters = map.Characters.ToList();
                if (characters.Any(character => character.Status.IsFlagStat(FlagStatType.OverDrive)))
                {
                    characters.RemoveAll(character => !character.Status.IsFlagStat(FlagStatType.OverDrive));
                }

                var minWaitTime = characters.Min(character =>
                    character.Status.Stats.CurrentMaxWaitTime - character.Status.Stats.CurrentWaitTime);
                minWaitTime = Mathf.Min(minWaitTime,
                    _turnWaitTime.MaxValue.CurrentValue - _turnWaitTime.Value.CurrentValue);
                _turnWaitTime.Gain(minWaitTime);

                if (_turnWaitTime.IsFull())
                {
                    _turnInLevel.Value++;
                    _onTurnChanged.OnNext(Unit.Default);
                    Debug.Log($"[Turn]Start turn in level:{_turnInLevel.Value})\nCharacters:{map.Characters.Count}");
                    map.UpdateTurn(_turnInLevel.Value);
                }

                foreach (var character in characters)
                {
                    if (characters.Any(character => character.Status.IsFlagStat(FlagStatType.OverDrive)) &&
                        !character.Status.IsFlagStat(FlagStatType.OverDrive))
                        continue;

                    if (_turnWaitTime.IsFull())
                    {
                        character.UpdateTurn();
                    }

                    character.Status.AddWaitTime(minWaitTime);
                    if (character.Status.IsWaitTimeFull())
                    {
                        if (character.State != CharacterState.Wait)
                            continue;

                        if (!character.Status.IsFlagStat(FlagStatType.CannotAct) && !character.IsDead)
                        {
                            if (character.IsPlayer && Settings.AutoSave.CurrentValue)
                            {
                                Globals.GameManager.Save();
                            }

                            Log.Debug($"[Turn]{character.GetName(map.Player)} think...");
                            try
                            {
                                await character.DoNextAction(gameManager, map, _input)
                                    .AttachExternalCancellation(_cancellationTokenSource.Token);
                            }
                            catch (OperationCanceledException e)
                            {
                                Log.Error(e);
                            }
                        }
                        else
                        {
                            character.CancelChargeAction();
                            Log.Debug($"[Turn]{character.GetName(map.Player)} cannot act.");
                        }

                        if (map.IsEventExecuting)
                        {
                            await UniTask.WaitWhile(() => map.IsEventExecuting);
                        }
                    }

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        _isRunning = false;
                        _runCompletionSource.TrySetResult();
                        Log.Debug("[Turn]Loop canceled.");
                        return;
                    }
                }

                foreach (var character in characters.Where(character => character.Status.IsWaitTimeFull()))
                {
                    if (character.State != CharacterState.Wait && character.State != CharacterState.Finish)
                    {
                        await UniTask.WaitUntil(() =>
                            character.State == CharacterState.Wait || character.State == CharacterState.Finish);
                    }

                    character.Status.ResetWaitTime();
                    character.SetWaitState();
                }

                if (_turnWaitTime.IsFull())
                {
                    _turnWaitTime.Set(0);
                }
            }

            _isRunning = false;
            _runCompletionSource.TrySetResult();
        }

        public async UniTask Stop()
        {
            if (!_isRunning) return;
            _cancellationTokenSource.Cancel();
            await _runCompletionSource.Task;
            Log.Debug("[Turn]Stop");
        }
    }
}