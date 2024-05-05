#nullable enable
using Cysharp.Threading.Tasks;
using R3;
using Scripts.Model.Action;
using Scripts.Model.Characters.Effect;
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
        private CharacterControllInputReceiver _receiver;
        private IntelligentDashController _intelligentDashController = new IntelligentDashController();
        public PlayerBehavior(CharacterControllInputReceiver receiver)
        {
            _receiver = receiver;
        }
        public bool IsDashingStraight(bool started, IAction action)
        {
            return GameManager.IsDash() && !started && action is Move;
        }
        public async UniTask<IAction> GenerateNextAction(IHasBehavior character)
        {
            await _intelligentDashController.Wait(character);

            UniTask<(Move action, bool isStarted)> moveTask = _receiver.OnMoveInputReceived.WaitAsync();
            UniTask<Skill> skillTask = _receiver.OnSkillActionReceived.WaitAsync();

            _receiver.ReadInput();

            var firstCompletedTask = await UniTask.WhenAny(moveTask, skillTask);
            while (true)
            {
                switch (firstCompletedTask.winArgumentIndex)
                {
                    case 0:
                        (Move move, bool started) = firstCompletedTask.result1;
                        if (GameManager.IsNoMove())
                        {
                            character.Turn(move.Direction);
                        }
                        else
                        {
                            if (Settings.IntelligentDash.Value && IsDashingStraight(started, move))
                            {
                                move = _intelligentDashController.Filter(move, character);
                            }
                            else
                            {
                                _intelligentDashController.Reset();
                            }

                            if (move.Doable(character))
                            {
                                return move;
                            }
                        }
                        break;
                    case 1:
                        Skill skill = firstCompletedTask.result2;
                        IAction action = new UseSkill(skill, character.CurrentDirection);

                        _intelligentDashController.Reset();

                        if (action.Doable(character))
                        {
                            return action;
                        }
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }

                moveTask = _receiver.OnMoveInputReceived.WaitAsync();
                skillTask = _receiver.OnSkillActionReceived.WaitAsync();
                firstCompletedTask = await UniTask.WhenAny(moveTask, skillTask);
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
        public Move Filter(Move move, IHasBehavior character)
        {
            HashSet<Direction8> canMoveDirections = DirectionMethods.AllDirections.Where(direction => character.CanMove(direction)).ToHashSet();
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
