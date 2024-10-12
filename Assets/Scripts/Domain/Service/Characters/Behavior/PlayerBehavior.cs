#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Domain.Service.Action;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal sealed class PlayerBehavior : ICharacterBehavior
    {
        private readonly IntelligentDashController _intelligentDashController = new();
        private readonly CharacterControlInputReceiver _receiver;
        public BehaviorData BehaviorData => new();
        private readonly Subject<OnItemSelectMessage> _onItemSelect = new();
        public Observable<OnItemSelectMessage> OnItemSelect => _onItemSelect;
        private (Location, Vector2Int)? _homePosition;

        public PlayerBehavior(CharacterControlInputReceiver receiver)
        {
            _receiver = receiver;
        }

        public BehaviorMemento Serialize()
        {
            return new BehaviorMemento(BehaviorData, _homePosition, null);
        }

        public bool WanderAround => true;

        public async UniTask<IAction> GenerateNextAction(IHasBehavior character, IGameManager gameManager, IMap map,
            IInput input)
        {
            Log.Debug("[Think] Start waiting input...");
            if (input.IsDash()) await _intelligentDashController.Wait(character, map);

            UniTask<(Move action, bool isStarted)> moveTask = _receiver.OnMoveInputReceived.WaitAsync();
            var useItemTask = _receiver.OnUseItemActionReceived.WaitAsync();
            var throwItemTask = _receiver.OnThrowItemActionReceived.WaitAsync();
            var dropItemTask = _receiver.OnDropItemActionReceived.WaitAsync();
            var doNothingTask = _receiver.OnDoNothingActionReceived.WaitAsync();
            var renameItemTask = _receiver.OnRenameItemActionReceived.WaitAsync();

            _receiver.ReadInput();

            var firstCompletedTask = await UniTask.WhenAny(moveTask, useItemTask, throwItemTask, dropItemTask, doNothingTask, renameItemTask);
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
                                move = _intelligentDashController.Filter(move, character, started, map, input);

                            var swap = new Swap(move.Direction);
                            var eventEntity =
                                map.GetEventEntityAt(character.CurrentPosition + move.Direction.Vector(),
                                    EntityLayer.Middle);
                            character.Turn(move.Direction);
                            if (move.Doable(character, map))
                                return move;
                            if (eventEntity != null)
                            {
                                var choices = new List<string>();
                                var firstChoiceIndex = 0;
                                if (swap.Doable(character, map))
                                {
                                    choices.Add("入れ替わる");
                                    firstChoiceIndex += 1;
                                }

                                var executableEvents = eventEntity.Events.Where(e => e.CanExecuteEvent()).ToList();
                                foreach (var eventData in executableEvents)
                                {
                                    choices.Add(eventData.ChoiceText);
                                }

                                if (eventEntity.CanBeCanceled)
                                {
                                    choices.Add("やめる");
                                }

                                var choiceIndex = 0;
                                if (choices.Count > 1)
                                {
                                    choiceIndex =
                                        await gameManager.GetChoice(eventEntity.ChoiceMessage, choices.ToArray());
                                }

                                switch (choices[choiceIndex])
                                {
                                    case "入れ替わる":
                                        return swap;
                                    case "やめる":
                                        break;
                                    default:
                                        await executableEvents[choiceIndex - firstChoiceIndex]
                                            .DoEvent(gameManager, map);
                                        return new DoNothing();
                                }
                            }
                            else if (swap.Doable(character, map))
                                return swap;
                        }

                        break;
                    case 1:
                        var itemIndex = firstCompletedTask.result2;
                        var item = itemIndex == null ? null : character.Inventory.GetItem(itemIndex.Value);
                        IAction action;

                        if (item == null)
                            action = new UseSkill(character.Skills[0], character.CurrentDirection);
                        else
                            action = new UseItem(item, character.CurrentDirection);

                        if (action.Doable(character, map)) return action;
                        break;
                    case 2:
                        itemIndex = firstCompletedTask.result3;
                        item = itemIndex == null ? null : character.Inventory.GetItem(itemIndex.Value);
                        if (item != null)
                        {
                            action = new ThrowItem(item, character.CurrentDirection);
                            if (action.Doable(character, map)) return action;
                        }

                        break;
                    case 3:
                        itemIndex = firstCompletedTask.result4;
                        if (itemIndex != null)
                        {
                            action = new DropItem(itemIndex.Value);
                            if (action.Doable(character, map)) return action;
                        }
                        break;
                    case 4:
                        await UniTask.Yield();
                        return new DoNothing();
                    case 5:
                        itemIndex = firstCompletedTask.result6;
                        if (itemIndex != null)
                        {
                            item = character.Inventory.GetItem(itemIndex.Value);
                            if (item == null) break;
                            item.Rename(await gameManager.GetTextInput());
                        }
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }

                moveTask = _receiver.OnMoveInputReceived.WaitAsync();
                useItemTask = _receiver.OnUseItemActionReceived.WaitAsync();
                throwItemTask = _receiver.OnThrowItemActionReceived.WaitAsync();
                dropItemTask = _receiver.OnDropItemActionReceived.WaitAsync();
                doNothingTask = _receiver.OnDoNothingActionReceived.WaitAsync();
                renameItemTask = _receiver.OnRenameItemActionReceived.WaitAsync();
                firstCompletedTask = await UniTask.WhenAny(moveTask, useItemTask, throwItemTask, dropItemTask, doNothingTask, renameItemTask);
            }
        }

        public void KnowLocationOf(IHasBehavior self, IActorOfEffect target) {}

        public async UniTask<IItem?> SelectItem(IInventory inventory, params int[] disabledItemIds)
        {
            _onItemSelect.OnNext(new OnItemSelectMessage(true, disabledItemIds));

            int? index;
            do
            {
                index = await _receiver.OnUseItemActionReceived.WaitAsync();
            } while (index.HasValue && disabledItemIds.Contains(index.Value));

            _onItemSelect.OnNext(new OnItemSelectMessage(false, new int[0]));
            return index == null ? null : inventory.GetItem(index.Value);
        }
    }
}