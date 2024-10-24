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
using UnityEngine;

namespace Domain.Service.Events
{
    public class CharacterEvent : IEvent
    {
        public bool IsPlayerOnly => false;
        private readonly Func<ICharacter, bool> _canExecuteEvent;
        private readonly Func<ICharacter, IGameManager, IMap, UniTask> _doEvent;
        public CharacterEvent(Func<ICharacter, bool> canExecuteEvent, Func<ICharacter, IGameManager, IMap, UniTask> doEvent)
        {
            _canExecuteEvent = canExecuteEvent;
            _doEvent = doEvent;
        }
        public bool CanExecuteEvent(ICharacter character) => _canExecuteEvent(character);
        public async UniTask<bool> DoEvent(ICharacter character, IGameManager gameManager, IMap map)
        {
            if (_canExecuteEvent(character))
            {
                await _doEvent(character, gameManager, map);
                return true;
            }
            return false;
        }
    }

    internal class PlayerEvent : IEvent
    {
        public bool IsPlayerOnly => true;
        public readonly string? ChoiceMessage;
        public readonly bool CanBeCanceled;
        public readonly IReadOnlyList<PlayerChoiceEvent> Events;

        public PlayerEvent(string? choiceMessage, bool canBeCanceled, List<PlayerChoiceEvent> choices)
        {
            ChoiceMessage = choiceMessage;
            CanBeCanceled = canBeCanceled;
            Events = choices;
        }
        public bool CanExecuteEvent(ICharacter character) => Events.Where(e => e.CanExecuteEvent(character)).Any();
        public async UniTask<bool> DoEvent(ICharacter character, IGameManager gameManager, IMap map)
        {
            var choices = new List<string>();

            var executableEvents = Events.Where(e => e.CanExecuteEvent(character)).ToList();
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
        }

        public async UniTask<IAction?> DoAction(ICharacter character, IGameManager gameManager, IMap map, Swap swap)
        {
            var executableEvents = Events.Where(e => e.CanExecuteEvent(character)).ToList();
            if (!executableEvents.Any())
            {
                return null;
            }

            var choices = new List<string>();
            var firstChoiceIndex = 0;
            if (swap.Doable(character, map))
            {
                choices.Add("入れ替わる");
                firstChoiceIndex += 1;
            }

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
                    return null;
                default:
                    await executableEvents[choiceIndex - firstChoiceIndex]
                        .DoEvent(character, gameManager, map);
                    return new DoNothing();
            }
        }
    }
    public class PlayerChoiceEvent : IEvent
    {
        public bool IsPlayerOnly => true;
        public string ChoiceText { get; init; }
        private readonly Func<ICharacter, bool> _canExecuteEvent;
        private readonly Func<IGameManager, IMap, UniTask> _doEvent;
        public PlayerChoiceEvent(string choiceText, Func<ICharacter, bool> canExecuteEvent, Func<IGameManager, IMap, UniTask> doEvent)
        {
            ChoiceText = choiceText;
            _canExecuteEvent = canExecuteEvent;
            _doEvent = doEvent;
        }
        public bool CanExecuteEvent(ICharacter character) => _canExecuteEvent(character);
        public async UniTask<bool> DoEvent(ICharacter character, IGameManager gameManager, IMap map)
        {
            if (CanExecuteEvent(character))
            {
                await _doEvent(gameManager, map);
                return true;
            }
            return false;
        }
    }
}