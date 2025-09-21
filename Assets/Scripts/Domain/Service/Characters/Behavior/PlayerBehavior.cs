#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Domain.Service.Action;
using Domain.Service.Events;
using Domain.Service.Logs;
using R3;
using Unity.Logging;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Characters.Behavior
{
    internal sealed class PlayerBehavior : ICharacterBehavior
    {
        private readonly IntelligentDashController _intelligentDashController = new();
        private readonly CharacterControlInputReceiver _receiver;
        public BehaviorData BehaviorData => new();
        private readonly Subject<OnItemSelectMessage> _onItemSelect = new();
        public Observable<OnItemSelectMessage> OnItemSelect => _onItemSelect;
        private Option<Location> _homeLocation;

        private enum InputType
        {
            Move,
            UseItem,
            ThrowItem,
            SwapItem,
            DoNothing,
            RenameItem
        }

        public PlayerBehavior(CharacterControlInputReceiver receiver)
        {
            _receiver = receiver;
        }

        public BehaviorMemento Serialize()
        {
            return new BehaviorMemento(BehaviorData, _homeLocation, Option<BehaviorState>.None, Option<Location>.None);
        }

        public static BehaviorMemento Build()
        {
            return new BehaviorMemento(new BehaviorData(), Option<Location>.None, Option<BehaviorState>.None, Option<Location>.None);
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
                        var (move, started) = result.move!.Value;

                        var destination = character.Entity.CurrentPosition + move.Direction.Vector();
                        var playerEventEntity = map.GetPlayerEventEntityFastAt(destination, EntityLayer.Middle);
                        if (input.IsNoMove() || character.Status.IsFlagStat(FlagStatType.CannotMove))
                        {
                            character.Turn(move.Direction);
                            var (eventAction, _) = await TryGetPlayerEventAction(character, gameManager, map, playerEventEntity, new Swap(move.Direction));
                            if (eventAction != null)
                                return eventAction;
                        }
                        else
                        {
                            if (Settings.GlobalSettings.IntelligentDash.CurrentValue)
                                move = _intelligentDashController.Filter(move, character, started, map, input);
                            var swap = new Swap(move.Direction);

                            character.Turn(move.Direction);
                            if (move.Doable(character, map))
                                return move;
                            var (eventAction, anyEventCanExecute) = await TryGetPlayerEventAction(character, gameManager, map, playerEventEntity, swap);
                            if (eventAction != null)
                                return eventAction;
                            if (!anyEventCanExecute && swap.Doable(character, map))
                                return swap;
                        }
                        break;
                    case InputType.UseItem:
                        var focus = result.focus!;
                        var item = focus.GetItem(character.Inventory, map);
                        IAction action;

                        if (item == null)
                            action = new UseSkill(character.Skills[0], character.CurrentDirection);
                        else
                            action = new UseItem(item, character.CurrentDirection);

                        if (action.Doable(character, map)) return action;
                        break;
                    case InputType.ThrowItem:
                        focus = result.focus!;
                        item = focus.GetItem(character.Inventory, map);
                        if (item != null)
                        {
                            action = new ThrowItem(item, character.CurrentDirection);
                            if (action.Doable(character, map)) return action;
                        }

                        break;
                    case InputType.SwapItem:
                        focus = result.focus!;
                        if (focus.IsEmpty)
                        {
                            break;
                        }
                        ItemFocus focus2;
                        if (focus.IsGroundItem && character.Inventory.HasEmptySpace())
                        {
                            var emptyIndex = character.Inventory.GetItemIndex(null);
                            focus2 = new ItemFocus(emptyIndex);
                        }
                        else
                        {
                            var disabledItemIndexes = new List<ItemFocus> { focus };
                            item = focus.GetItem(character.Inventory, map);
                            if (item != null)
                            {
                                if (item.ItemStorage.IsSome)
                                {
                                    for (int i = 0; i < item.ItemStorage.Value.Capacity; i++)
                                    {
                                        disabledItemIndexes.Add(new ItemFocus(focus.Index, i));
                                    }
                                }
                                else if (focus.SubIndex >= 0)
                                {
                                    disabledItemIndexes.Add(new ItemFocus(focus.Index));
                                }
                            }
                            focus2 = await SelectItem("入れ替え先を選択してください", character.Inventory, map, disabledItemIndexes.ToArray());

                            if (focus == focus2)
                            {
                                break;
                            }

                            if (focus2.IsEmpty)
                            {
                                break;
                            }
                        }

                        if (focus.IsGroundItem)
                        {
                            action = new DropItem(focus2);
                            if (action.Doable(character, map)) return action;
                        }
                        else
                        {
                            if (focus2.IsGroundItem)
                            {
                                action = new DropItem(focus);
                                if (action.Doable(character, map)) return action;
                            }
                            else
                            {
                                var tempItem = character.Inventory.Replace(null, focus);
                                var temp2Item = character.Inventory.Replace(tempItem.Value, focus2);
                                character.Inventory.Replace(temp2Item.Value, focus);
                                return new DoNothing();
                            }
                        }
                        break;
                    case InputType.DoNothing:
                        await UniTask.Yield();
                        return new DoNothing();
                    case InputType.RenameItem:
                        focus = result.focus!;
                        item = focus.GetItem(character.Inventory, map);
                        if (item == null)
                            break;

                        var choices = new List<string>();
                        if (!item.IsInfoIdentified(map.Player))
                        {
                            choices.Add("このアイテムの種類に名前をつける");
                        }
                        if (item.CustomName.IsSome)
                        {
                            choices.Add("このアイテム単体の名前を変える");
                            choices.Add("このアイテム単体の名前をデフォルトに戻す");
                        }
                        else
                        {
                            choices.Add("このアイテム単体に名前をつける");
                        }
                        choices.Add("やめる");

                        var choice = await gameManager.GetChoice(null, choices.ToArray());
                        switch (choices[choice])
                        {
                            case "このアイテムの種類に名前をつける":
                                map.ItemPlaceholders.Rename(item.BaseName, await gameManager.GetTextInput());
                                break;
                            case "このアイテム単体に名前をつける":
                                item.Rename(await gameManager.GetTextInput());
                                break;
                            case "このアイテム単体の名前をデフォルトに戻す":
                                item.RevertToDefaultName();
                                break;
                            case "やめる":
                                break;
                        }
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }

                result = await InitializeTasks();
            }
        }

        private static async UniTask<(IAction? action, bool anyEventCanExecute)> TryGetPlayerEventAction(IHasBehavior character, IGameManager gameManager, IMap map, IHasPlayerEvent? playerEventEntity, Swap swap)
        {
            if (playerEventEntity == null || !playerEventEntity.Events.Any(e => e.CanExecuteEvent(map.Player, map)))
                return (null, false);
            var eventAction = await playerEventEntity.Events.DoAction(map.Player, gameManager, map, swap);
            if (eventAction != null && eventAction.Doable(character, map))
                return (eventAction, true);
            return (null, true);
        }

        private async UniTask<(InputType type, (Move action, bool isStarted)? move, ItemFocus? focus)> InitializeTasks()
        {
            var cancellationToken = new CancellationTokenSource();
            UniTask<(Move action, bool isStarted)> moveTask =
                _receiver.OnMoveInputReceived.WaitAsync(cancellationToken.Token);
            var useItemTask = _receiver.OnUseItemActionReceived.WaitAsync(cancellationToken.Token);
            var throwItemTask = _receiver.OnThrowItemActionReceived.WaitAsync(cancellationToken.Token);
            var swapItemTask = _receiver.OnSwapItemActionReceived.WaitAsync(cancellationToken.Token);
            var doNothingTask = _receiver.OnDoNothingActionReceived.WaitAsync(cancellationToken.Token);
            var renameItemTask = _receiver.OnRenameItemActionReceived.WaitAsync(cancellationToken.Token);

            var tasks = await UniTask.WhenAny(moveTask, useItemTask, throwItemTask, swapItemTask, doNothingTask,
                renameItemTask);
            cancellationToken.Cancel();
            return tasks.winArgumentIndex switch
            {
                0 => (InputType.Move, tasks.result1, null),
                1 => (InputType.UseItem, null, tasks.result2),
                2 => (InputType.ThrowItem, null, tasks.result3),
                3 => (InputType.SwapItem, null, tasks.result4),
                4 => (InputType.DoNothing, null, null),
                5 => (InputType.RenameItem, null, tasks.result6),
                _ => throw new IndexOutOfRangeException()
            };
        }

        public void KnowLocationOf(Location location)
        {
        }

        public async UniTask<ItemFocus> SelectItem(string text, IInventory inventory, IMap map, params ItemFocus[] disabledItemIndexes)
        {
            _onItemSelect.OnNext(new OnItemSelectMessage(text, true, disabledItemIndexes));

            ItemFocus? focus;
            do
            {
                focus = await _receiver.OnUseItemActionReceived.WaitAsync();
            } while (!focus.IsEmpty && disabledItemIndexes.Contains(focus));

            _onItemSelect.OnNext(new OnItemSelectMessage(text, false, new ItemFocus[0]));
            return focus;
        }
    }
}