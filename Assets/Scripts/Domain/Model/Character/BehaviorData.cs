#nullable enable
using System;
using Sirenix.OdinInspector;

namespace Domain.Model.Character
{
    [Serializable]
    public class BehaviorData
    {
        public bool wanderAround = true;
        public MoveTypeWhenDiscoveringTarget Default = MoveTypeWhenDiscoveringTarget.Chase;
        public bool PrioritizeMovement = false;
        public bool UseTopBound = false;
        [ShowIf("UseTopBound")] public MoveTypeWhenDiscoveringTarget greaterThanTopBound = MoveTypeWhenDiscoveringTarget.Chase;
        [ShowIf("UseTopBound"), MinValue(0)] public float distanceTopBound = 6f;
        [ShowIf("UseTopBound")] public bool PrioritizeMovementWhenDistanceGreaterThanTopBound = false;
        public bool UseBottomBound = false;
        [ShowIf("UseBottomBound"), MinValue(0)] public float distanceBottomBound = 3f;
        [ShowIf("UseBottomBound")] public MoveTypeWhenDiscoveringTarget lessThanBottomBound = MoveTypeWhenDiscoveringTarget.Chase;
        [ShowIf("UseBottomBound")] public bool PrioritizeMovementWhenDistanceLessThanBottomBound = false;

        public bool PrioritizeEnemiesOverLeaders = false;
    }
}