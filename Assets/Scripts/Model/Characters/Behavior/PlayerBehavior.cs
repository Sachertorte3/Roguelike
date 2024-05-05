using Cysharp.Threading.Tasks;
using Scripts.Model.Action;
using Scripts.Model.Setting;
using Scripts.Utilities;
using UnityEngine;

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
        public async UniTask<IAction> GenerateNextAction(IHasBehavior character)
        {
            await _intelligentDashController.Wait(character);
            UniTask<IAction> actionTask = _actionReceiver.ReceivedAction.WaitAsync();
            _actionReceiver.ReadInput();
            while (true)
            {
                IAction action = await actionTask;
                if (action.Doable(character))
                {
                    return action switch
                    {
                        Move move => Settings.IntelligentDash.Value? await _intelligentDashController.Filter(move, character): move,
                        _ => action
                    };
                }
                actionTask = _actionReceiver.ReceivedAction.WaitAsync();
            }
        }
    }
    internal sealed class IntelligentDashController
    {
        public async UniTask Wait(IHasBehavior character)
        {
            if (character.CanMove(character.CurrentDirection.Reverse()) &&
                (
                    (character.CanMove(character.CurrentDirection.Rotate90Clockwise()) && !character.CanMove(character.CurrentDirection.Reverse().Rotate45AntiClockwise()))
                    || (character.CanMove(character.CurrentDirection.Rotate90AntiClockwise()) && !character.CanMove(character.CurrentDirection.Reverse().Rotate45Clockwise()))
                )
            )
            {
                await UniTask.Delay(Settings.DashPauseMilliseconds.Value);
            }
        }
        public async UniTask<Move> Filter(Move move, IHasBehavior character)
        {
            return move;
        }
    }
}
