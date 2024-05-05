using Cysharp.Threading.Tasks;
using Scripts.Model.Action;
using Scripts.Model.Setting;
using Scripts.Utilities;

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
            UniTask<IAction> actionTask = _actionReceiver.ReceivedAction.WaitAsync();
            _actionReceiver.ReadInput();
            while (true)
            {
                IAction action = await actionTask;
                if (action.Doable(character))
                {
                    return action switch
                    {
                        Move move => await _intelligentDashController.Filter(move, character),
                        _ => action
                    };
                }
                actionTask = _actionReceiver.ReceivedAction.WaitAsync();
            }
        }
    }
    internal sealed class IntelligentDashController
    {
        public async UniTask<Move> Filter(Move move, IHasBehavior character)
        {
            if (character.CanMove(move.Direction.Reverse()) &&
                (
                    (character.CanMove(move.Direction.Rotate90Clockwise()) && !character.CanMove(move.Direction.Reverse().Rotate45AntiClockwise()))
                    || (character.CanMove(move.Direction.Rotate90AntiClockwise()) && !character.CanMove(move.Direction.Reverse().Rotate45Clockwise()))
                )
            )
            {
                await UniTask.Delay(Settings.MoveMilliseconds.Value);
            }
            return move;
        }
    }
}
