#nullable enable
using Cysharp.Threading.Tasks;
using Scripts.Model.Action;
using Scripts.Model.Setting;
using Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Logging;

namespace Scripts.Model.Characters.Behavior
{
    internal sealed class PlayerBehavior : ICharacterBehavior
    {
        private ActionReceiver _actionReceiver;
        private IntelligentDashController _intelligentDashController = new IntelligentDashController();
        public PlayerBehavior(ActionReceiver actionReceiver)
        {
            _actionReceiver = actionReceiver;
        }
        public bool IsDashingStraight(bool started, IAction action)
        {
            return GameManager.IsDash() && !started && action is Move;
        }
        public async UniTask<IAction> GenerateNextAction(IHasBehavior character)
        {
            await _intelligentDashController.Wait(character);
            UniTask<(IAction action, bool isStarted)> actionTask = _actionReceiver.ReceivedAction.WaitAsync();
            _actionReceiver.ReadInput();
            while (true)
            {
                (IAction action, bool started) = await actionTask;
                if (Settings.IntelligentDash.Value && IsDashingStraight(started, action))
                {
                    Move? newMove = await _intelligentDashController.Filter((Move)action, character);
                    if (newMove != null)
                    {
                        action = newMove;
                    }
                }
                else
                {
                    _intelligentDashController.Reset();
                }

                if (action.Doable(character))
                {
                    return action;
                }
                actionTask = _actionReceiver.ReceivedAction.WaitAsync();
            }
        }
    }
    internal sealed class IntelligentDashController
    {
        private bool _inStraightway = false;
        public async UniTask Wait(IHasBehavior character)
        {
            if (character.CanMove(character.CurrentDirection) && character.CanMove(character.CurrentDirection.Reverse()) &&
                (
                    (character.CanMove(character.CurrentDirection.Rotate90Clockwise()) && !character.CanMove(character.CurrentDirection.Reverse().Rotate45AntiClockwise()))
                    || (character.CanMove(character.CurrentDirection.Rotate90AntiClockwise()) && !character.CanMove(character.CurrentDirection.Reverse().Rotate45Clockwise()))
                )
            )
            {
                await UniTask.Delay(Settings.DashPauseMilliseconds.Value);
            }
        }
        public async UniTask<Move?> Filter(Move move, IHasBehavior character)
        {
            HashSet < Direction8 > canMoveDirections = DirectionMethods.AllDirections.Where(direction => character.CanMove(direction)).ToHashSet();
            _inStraightway = canMoveDirections.Count() == 2;
            if (_inStraightway)
            {
                Direction8 lastMoveDirection = character.CurrentDirection;
                if (!canMoveDirections.Remove(lastMoveDirection.Reverse()))
                {
                    Log.Info($"The possible position of the previous turn based on the character's direction({character.CurrentDirection}) is not a passage.");
                    return move;
                }
                return new Move(canMoveDirections.First());
            }
            return move;
        }
        public void Reset()
        {
            _inStraightway = false;
        }
    }
}
