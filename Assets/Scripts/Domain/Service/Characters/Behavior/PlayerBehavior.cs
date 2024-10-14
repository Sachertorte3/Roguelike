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
        private enum InputType
        {
            Move,
            UseItem,
            ThrowItem,
            DropItem,
            DoNothing,
            RenameItem
        }

        public PlayerBehavior(CharacterControlInputReceiver receiver)
        {
            _receiver = receiver;
        }

        public BehaviorMemento Serialize()
        {
            return new BehaviorMemento(BehaviorData, _homePosition, null, null);
        }

        public static BehaviorMemento Build()
        {
            return new BehaviorMemento(new BehaviorData(), null, null, null);
        }

        public bool WanderAround => true;

        public async UniTask<IAction> GenerateNextAction(IHasBehavior character, IGameManager gameManager, IMap map,
            IInput input)
        {
            Log.Debug("[Think] Start waiting input...");
            if (input.IsDash()) await _intelligentDashController.Wait(character, map);

            var tasks = InitializeTasks();
            _receiver.ReadInput();
            var result = await tasks;

            while (true)
            {
                switch (result.type)
                {
                    case InputType.Move:
                        var (move, started) = result.move.Value;
                        if (input.IsNoMove())
                        {
                            character.Turn(move.Direction);
                            break;
                        }

                        if (Settings.IntelligentDash.Value)
                            move = _intelligentDashController.Filter(move, character, started, map, input);

                        var swap = new Swap(move.Direction);
                        var destination = character.CurrentPosition + move.Direction.Vector();
                        var eventEntity = map.GetEventEntityAt(destination, EntityLayer.Middle);
                        character.Turn(move.Direction);

                        if (move.Doable(character, map))
                            return move;
                        else if (eventEntity != null)
                        {
                            var eventAction = await ChoiceEvent(character, gameManager, map, swap, eventEntity);
                            if (eventAction != null && eventAction.Doable(character, map))
                                return eventAction;
                        }
                        else if (swap.Doable(character, map))
                            return swap;
                        break;
                    case InputType.UseItem:
                        var itemIndex = result.itemIndex;
                        var item = itemIndex == null ? null : character.Inventory.GetItem(itemIndex.Value);
                        IAction action;

                        if (item == null)
                            action = new UseSkill(character.Skills[0], character.CurrentDirection);
                        else
                            action = new UseItem(item, character.CurrentDirection);

                        if (action.Doable(character, map)) return action;
                        break;
                    case InputType.ThrowItem:
                        itemIndex = result.itemIndex;
                        item = itemIndex == null ? null : character.Inventory.GetItem(itemIndex.Value);
                        if (item != null)
                        {
                            action = new ThrowItem(item, character.CurrentDirection);
                            if (action.Doable(character, map)) return action;
                        }

                        break;
                    case InputType.DropItem:
                        itemIndex = result.itemIndex;
                        if (itemIndex != null)
                        {
                            action = new DropItem(itemIndex.Value);
                            if (action.Doable(character, map)) return action;
                        }
                        break;
                    case InputType.DoNothing:
                        await UniTask.Yield();
                        return new DoNothing();
                    case InputType.RenameItem:
                        itemIndex = result.itemIndex;
                        if (itemIndex != null)
                        {
                            item = character.Inventory.GetItem(itemIndex.Value);
                            if (item == null) break;
                            map.ItemDatabase.Rename(item.BaseName, await gameManager.GetTextInput());
                        }
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }

                result = await InitializeTasks();
            }
        }

        private async UniTask<(InputType type, (Move action, bool isStarted)? move, int? itemIndex)> InitializeTasks()
        {
            UniTask<(Move action, bool isStarted)> moveTask = _receiver.OnMoveInputReceived.WaitAsync();
            var useItemTask = _receiver.OnUseItemActionReceived.WaitAsync();
            var throwItemTask = _receiver.OnThrowItemActionReceived.WaitAsync();
            var dropItemTask = _receiver.OnDropItemActionReceived.WaitAsync();
            var doNothingTask = _receiver.OnDoNothingActionReceived.WaitAsync();
            var renameItemTask = _receiver.OnRenameItemActionReceived.WaitAsync();

            var tasks = await UniTask.WhenAny(moveTask, useItemTask, throwItemTask, dropItemTask, doNothingTask, renameItemTask);
            return tasks.winArgumentIndex switch
            {
                0 => (InputType.Move, tasks.result1, null),
                1 => (InputType.UseItem, null, tasks.result2),
                2 => (InputType.ThrowItem, null, tasks.result3),
                3 => (InputType.DropItem, null, tasks.result4),
                4 => (InputType.DoNothing, null, null),
                5 => (InputType.RenameItem, null, tasks.result6),
                _ => throw new IndexOutOfRangeException()
            };
        }

        private async UniTask<IAction?> ChoiceEvent(IHasBehavior character, IGameManager gameManager, IMap map, Swap swap, IEventEntity eventEntity)
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
            return null;
        }

        public void KnowLocationOf(Vector2Int position) { }

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