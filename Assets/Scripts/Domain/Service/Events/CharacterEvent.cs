#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Action;

namespace Domain.Service.Events
{
    public class CharacterEvent : IEvent
    {
        public bool IsPlayerOnly => false;
        public Func<bool> CanExecuteEvent { get; init; }
        private readonly Func<ICharacter, IGameManager, IMap, UniTask> _doEvent;
        public Func<ICharacter, IGameManager, IMap, UniTask<bool>> DoEvent => async (character, gameManager, map) =>
        {
            if (CanExecuteEvent())
            {
                await _doEvent(character, gameManager, map);
                return true;
            }
            return false;
        };
        public CharacterEvent(Func<bool> canExecuteEvent, Func<ICharacter, IGameManager, IMap, UniTask> doEvent)
        {
            CanExecuteEvent = canExecuteEvent;
            _doEvent = doEvent;
        }
    }

    internal class PlayerEvent : IEvent
    {
        public bool IsPlayerOnly => true;
        public Func<bool> CanExecuteEvent => () => Events.Any(e => e.CanExecuteEvent());
        public Func<ICharacter, IGameManager, IMap, UniTask<bool>> DoEvent { get; init; }
        public readonly string? ChoiceMessage;
        public readonly bool CanBeCanceled;
        public readonly IReadOnlyList<PlayerChoiceEvent> Events;
        public readonly string ChoiceText;

        public PlayerEvent(string? choiceMessage, bool canBeCanceled, List<PlayerChoiceEvent> choices)
        {
            ChoiceMessage = choiceMessage;
            CanBeCanceled = canBeCanceled;
            Events = choices;

            DoEvent = async (character, gameManager, map) =>
            {
                var choices = new List<string>();

                var executableEvents = Events.Where(e => e.CanExecuteEvent()).ToList();
                foreach (var eventData in executableEvents)
                {
                    choices.Add(eventData.ChoiceText);
                }

                if (CanBeCanceled)
                {
                    choices.Add("やめる");
                }

                var choiceIndex = 0;
                if (choices.Count > 1)
                {
                    choiceIndex = await gameManager.GetChoice(ChoiceMessage, choices.ToArray());
                }

                switch (choices[choiceIndex])
                {
                    case "やめる":
                        return false;
                    default:
                        await executableEvents[choiceIndex].DoEvent(character, gameManager, map);
                        return true;
                }
            };
        }

        public async UniTask<IAction?> DoAction(ICharacter character, IGameManager gameManager, IMap map, Swap swap)
        {
            var choices = new List<string>();
            var firstChoiceIndex = 0;
            if (swap.Doable(character, map))
            {
                choices.Add("入れ替わる");
                firstChoiceIndex += 1;
            }

            var executableEvents = Events.Where(e => e.CanExecuteEvent()).ToList();
            foreach (var eventData in executableEvents)
            {
                choices.Add(eventData.ChoiceText);
            }

            if (CanBeCanceled)
            {
                choices.Add("やめる");
            }

            var choiceIndex = 0;
            if (choices.Count > 1)
            {
                choiceIndex =
                    await gameManager.GetChoice(ChoiceMessage, choices.ToArray());
            }

            switch (choices[choiceIndex])
            {
                case "入れ替わる":
                    return swap;
                case "やめる":
                    break;
                default:
                    await executableEvents[choiceIndex - firstChoiceIndex]
                        .DoEvent(character, gameManager, map);
                    return new DoNothing();
            }
            return null;
        }
    }
    public class PlayerChoiceEvent : IEvent
    {
        public bool IsPlayerOnly => true;
        public string ChoiceText { get; init; }
        public Func<bool> CanExecuteEvent { get; init; }
        private readonly Func<ICharacter, IGameManager, IMap, UniTask> _doEvent;
        public Func<ICharacter, IGameManager, IMap, UniTask<bool>> DoEvent => async (character, gameManager, map) =>
        {
            if (CanExecuteEvent())
            {
                await _doEvent(character, gameManager, map);
                return true;
            }
            return false;
        };
        public PlayerChoiceEvent(string choiceText, Func<bool> canExecuteEvent, Func<ICharacter, IGameManager, IMap, UniTask> doEvent)
        {
            ChoiceText = choiceText;
            CanExecuteEvent = canExecuteEvent;
            _doEvent = doEvent;
        }
    }
}