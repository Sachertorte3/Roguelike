#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Action;

namespace Domain.Service.Events
{
    public class PlayerEvent : IPlayerEvent
    {
        public string? ChoiceMessage { get; init; }
        public IReadOnlyList<PlayerChoiceEvent> Events { get; init; }

        public PlayerEvent(string? choiceMessage, List<PlayerChoiceEvent> choices)
        {
            ChoiceMessage = choiceMessage;
            Events = choices;
        }

        public bool CanExecuteEvent(IPlayer player, IMap map)
        {
            return Events.Where(e => e.CanExecuteEvent(player, map)).Any();
        }

        public async UniTask<bool> DoEvent(IPlayer player, IGameManager gameManager, IMap map)
        {
            var choices = new List<string>();

            var executableEvents = Events.Where(e => e.CanExecuteEvent(player, map)).ToList();
            foreach (var eventData in executableEvents)
            {
                choices.Add(eventData.ChoiceText);
            }

            choices.Add("やめる");

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
            var executableEvents = Events.Where(e => e.CanExecuteEvent(player, map)).ToList();
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

            choices.Add("やめる");

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