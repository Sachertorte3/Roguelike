#nullable enable
using Domain.Model.Character;

namespace Domain.Service.Characters.Behavior
{
    public static class MoveTypeMethods
    {
        public static IBehaviorWhenDiscoveringTarget ToDiscoveredTargetBehavior(this MoveTypeWhenDiscoveringTarget moveType)
        {
            return moveType switch
            {
                MoveTypeWhenDiscoveringTarget.NoMove => new NoMove(),
                MoveTypeWhenDiscoveringTarget.Chase => new Chase(),
                MoveTypeWhenDiscoveringTarget.Wander => new Wander(),
                MoveTypeWhenDiscoveringTarget.Escape => new Escape(),
                _ => new NoMove(),
            };
        }
    }
}