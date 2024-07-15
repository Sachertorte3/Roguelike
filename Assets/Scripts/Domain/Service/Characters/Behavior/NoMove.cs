using System.Collections.Generic;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;

namespace Domain.Service.Characters.Behavior
{
    public sealed class NoMove : IUndiscoveredTargetBehavior
    {
        public IEnumerable<IAction> GenerateActionsDoable(IHasBehavior character, IMap world)
        {
            return new List<IAction>() { new DoNothing() };
        }
    }
}