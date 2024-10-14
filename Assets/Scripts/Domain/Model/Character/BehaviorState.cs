namespace Domain.Model.Character
{
    public enum BehaviorState
    {
        None,
        DiscoveringEnemy,
        DiscoveringLeader,
        ApproachingToObserve,
        ReturningHome,
        MovingToLastKnownEnemyPosition,
        MovingToLastKnownLeaderPosition,
        Wandering
    }
}