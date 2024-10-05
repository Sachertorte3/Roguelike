#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Map;

namespace Domain.Model
{
    public interface IHasEvent
    {
        public string? ChoiceMessage { get; }
        public bool CanBeCanceled { get; }
        public IReadOnlyList<EntityEvent> Events { get; }
    }

    public class EntityEvent
    {
        public readonly string ChoiceText;
        public readonly Func<bool> CanExecuteEvent;
        public readonly Func<IGameManager, IMap, UniTask> DoEvent;

        public EntityEvent(string choiceText, Func<bool> canExecuteEvent, Func<IGameManager, IMap, UniTask> doEvent)
        {
            ChoiceText = choiceText;
            CanExecuteEvent = canExecuteEvent;
            DoEvent = doEvent;
        }
    }
}