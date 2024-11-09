#nullable enable
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
    internal class PlayerEvent : IPlayerEvent
    {
        public readonly string? ChoiceMessage;
        public readonly bool CanBeCanceled;
        public readonly IReadOnlyList<PlayerChoiceEvent> Events;

        public PlayerEvent(string? choiceMessage, bool canBeCanceled, List<PlayerChoiceEvent> choices)
        {
            ChoiceMessage = choiceMessage;
            CanBeCanceled = canBeCanceled;
            Events = choices;
        }
        public bool CanExecuteEvent(IPlayer player) => Events.Where(e => e.CanExecuteEvent(player)).Any();
        public async UniTask<bool> DoEvent(IPlayer player, IGameManager gameManager, IMap map)
        {
            var choices = new List<string>();

            var executableEvents = Events.Where(e => e.CanExecuteEvent(player)).ToList();
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
                    await executableEvents[choiceIndex].DoEvent(player, gameManager, map);
                    return true;
            }
        }

        public async UniTask<IAction?> DoAction(IPlayer player, IGameManager gameManager, IMap map, IAction swap)
        {
            var executableEvents = Events.Where(e => e.CanExecuteEvent(player)).ToList();
            if (!executableEvents.Any())
            {
                return null;
            }

            var choices = new List<string>();
            var firstChoiceIndex = 0;
            if (swap.Doable(player.Character, map))
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
                        .DoEvent(player, gameManager, map);
                    return new DoNothing();
            }
        }
    }
}