#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Domain.Service.Action;
using Domain.Service.Items;
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
                        var (move, started) = result.move!.Value;

                        var destination = character.Entity.CurrentPosition + move.Direction.Vector();
                        var playerEventEntities = map.PlayerEventEntities.At(destination).On(EntityLayer.Middle);

                        if (input.IsNoMove() || character.Status.IsFlagStat(FlagStatType.CannotMove))
                        {
                            character.Turn(move.Direction);
                            if (playerEventEntities.Any() &&
                            playerEventEntities.All(e => e.Event.CanExecuteEvent(map.Player)))
                            {
                                foreach (var eventEntity in playerEventEntities)
                                {
                                    var eventAction = await eventEntity.Event.DoAction(map.Player, gameManager, map, new Swap(move.Direction));
                                    if (eventAction != null && eventAction.Doable(character, map))
                                        return eventAction;
                                    break;
                                }
                            }
                            break;
                        }

                        if (Settings.IntelligentDash.CurrentValue)
                            move = _intelligentDashController.Filter(move, character, started, map, input);
                        var swap = new Swap(move.Direction);
                        destination = character.Entity.CurrentPosition + move.Direction.Vector();

                        character.Turn(move.Direction);
                        if (move.Doable(character, map))
                            return move;
                        else if (playerEventEntities.Any() &&
                            playerEventEntities.All(e => e.Event.CanExecuteEvent(map.Player)))
                        {
                            foreach (var eventEntity in playerEventEntities)
                            {
                                var eventAction = await eventEntity.Event.DoAction(map.Player, gameManager, map, swap);
                                if (eventAction != null && eventAction.Doable(character, map))
                                    return eventAction;
                                break;
                            }
                        }
                        else if (swap.Doable(character, map))
                            return swap;
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
                    case InputType.DropItem:
                        focus = result.focus!;
                        if (focus.IsEmpty)
                            break;
                        if (focus.IsGroundItem)
                        {
                            if (character.TryPickUpItem(map, true))
                                return new DoNothing();
                            break;
                        }

                        action = new DropItem(focus);
                        if (action.Doable(character, map)) return action;
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

        private async UniTask<(InputType type, (Move action, bool isStarted)? move, ItemFocus? focus)> InitializeTasks()
        {
            var cancellationToken = new CancellationTokenSource();
            UniTask<(Move action, bool isStarted)> moveTask =
                _receiver.OnMoveInputReceived.WaitAsync(cancellationToken.Token);
            var useItemTask = _receiver.OnUseItemActionReceived.WaitAsync(cancellationToken.Token);
            var throwItemTask = _receiver.OnThrowItemActionReceived.WaitAsync(cancellationToken.Token);
            var dropItemTask = _receiver.OnDropItemActionReceived.WaitAsync(cancellationToken.Token);
            var doNothingTask = _receiver.OnDoNothingActionReceived.WaitAsync(cancellationToken.Token);
            var renameItemTask = _receiver.OnRenameItemActionReceived.WaitAsync(cancellationToken.Token);

            var tasks = await UniTask.WhenAny(moveTask, useItemTask, throwItemTask, dropItemTask, doNothingTask,
                renameItemTask);
            cancellationToken.Cancel();
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

        public void KnowLocationOf(Vector2Int position)
        {
        }

        public async UniTask<IItem?> SelectItem(IInventory inventory, IMap map, params ItemFocus[] disabledItemIndexes)
        {
            _onItemSelect.OnNext(new OnItemSelectMessage(true, disabledItemIndexes));

            ItemFocus? focus;
            do
            {
                focus = await _receiver.OnUseItemActionReceived.WaitAsync();
            } while (!focus.IsEmpty && disabledItemIndexes.Contains(focus));

            _onItemSelect.OnNext(new OnItemSelectMessage(false, new ItemFocus[0]));
            if (focus.IsEmpty)
                return null;
            return focus.GetItem(inventory, map);
        }
    }
}