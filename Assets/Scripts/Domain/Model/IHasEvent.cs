#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Model
{
    public interface IHasEvent
    {
        public IEvent Event { get; }
    }

    public interface IEvent
    {
        public bool IsPlayerOnly { get; }
        public Func<ICharacter, bool> CanExecuteEvent { get; }
        public Func<ICharacter, IGameManager, IMap, UniTask<bool>> DoEvent { get; }
    }
}