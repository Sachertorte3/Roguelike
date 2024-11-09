#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Service.Events
{
    public class PlayerChoiceEvent
    {
        public string ChoiceText { get; init; }
        private readonly Func<IPlayer, bool> _canExecuteEvent;
        private readonly Func<IGameManager, IMap, UniTask> _doEvent;
        public PlayerChoiceEvent(string choiceText, Func<IPlayer, bool> canExecuteEvent, Func<IGameManager, IMap, UniTask> doEvent)
        {
            ChoiceText = choiceText;
            _canExecuteEvent = canExecuteEvent;
            _doEvent = doEvent;
        }
        public bool CanExecuteEvent(IPlayer player) => _canExecuteEvent(player);
        public async UniTask<bool> DoEvent(IPlayer player, IGameManager gameManager, IMap map)
        {
            if (CanExecuteEvent(player))
            {
                await _doEvent(gameManager, map);
                return true;
            }
            return false;
        }
    }
}