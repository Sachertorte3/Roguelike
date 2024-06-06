#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Data;
using Data.Area;
using Data.Setting;
using Model.Domain.Action;
using Model.Domain.Effect;

namespace Model.Domain.Characters.Behavior
{
    public sealed class PlayerBehavior : ICharacterBehavior
    {
        private readonly IntelligentDashController _intelligentDashController = new();
        private readonly CharacterControllInputReceiver _receiver;

        public PlayerBehavior(CharacterControllInputReceiver receiver)
        {
            _receiver = receiver;
        }

        public async UniTask<IAction> GenerateNextAction(IHasBehavior character, IMap world, IInput input)
        {
            if (input.IsDash()) await _intelligentDashController.Wait(character, world);

            UniTask<(Move action, bool isStarted)> moveTask = _receiver.OnMoveInputReceived.WaitAsync();
            var useItemTask = _receiver.OnUseItemActionReceived.WaitAsync();
            var throwItemTask = _receiver.OnThrowItemActionReceived.WaitAsync();

            _receiver.ReadInput();

            var firstCompletedTask = await UniTask.WhenAny(moveTask, useItemTask, throwItemTask);
            while (true)
            {
                switch (firstCompletedTask.winArgumentIndex)
                {
                    case 0:
                        var (move, started) = firstCompletedTask.result1;
                        if (input.IsNoMove())
                        {
                            character.Turn(move.Direction);
                        }
                        else
                        {
                            if (Settings.IntelligentDash.Value)
                                move = _intelligentDashController.Filter(move, character, started, world, input);

                            if (move.Doable(character, world))
                                return move;
                            else
                            {
                                var swap = new Swap(move.Direction);
                                if (swap.Doable(character, world))
                                    return swap;
                            }
                            character.Turn(move.Direction);
                        }

                        break;
                    case 1:
                        var itemIndex = firstCompletedTask.result2;
                        var item = character.Inventory.GetItem(itemIndex);
                        IAction action;
                        if (item == null)
                            action = new UseSkill(new Skill(new SkillData(new LineArea(1, false), new AttackEffect(1))),
                                character.CurrentDirection);
                        else
                            action = new UseItem(itemIndex, character.CurrentDirection);

                        if (action.Doable(character, world)) return action;
                        break;
                    case 2:
                        itemIndex = firstCompletedTask.result3;
                        if (character.Inventory.GetItem(itemIndex) != null)
                        {
                            action = new ThrowItem(itemIndex, character.CurrentDirection);
                            if (action.Doable(character, world)) return action;
                        }

                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }

                moveTask = _receiver.OnMoveInputReceived.WaitAsync();
                useItemTask = _receiver.OnUseItemActionReceived.WaitAsync();
                throwItemTask = _receiver.OnThrowItemActionReceived.WaitAsync();
                firstCompletedTask = await UniTask.WhenAny(moveTask, useItemTask, throwItemTask);
            }
        }
    }
}