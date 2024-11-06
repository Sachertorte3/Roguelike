using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Position
{
    public class NearByCharacter : IEffectPosition
    {
        [MinValue(1)] public int NumberOfTarget = 1;
        public bool TargetAlly;
        public bool TargetEnemy;
        public bool TargetNeutral;
        public bool TargetSelf;
        public bool IsDirectional => false;

        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map)
        {
            var positions = new List<Vector2Int>();
            if (TargetSelf)
                positions.Add(actor.Entity.CurrentPosition);
            if (TargetAlly)
                positions.AddRange(map.Characters.In(actor.VisibleArea).FromAffiliation(actor, AffiliationType.Ally).Positions());
            if (TargetNeutral)
                positions.AddRange(map.Characters.In(actor.VisibleArea).FromAffiliation(actor, AffiliationType.Neutral).Positions());
            if (TargetEnemy)
                positions.AddRange(map.Characters.In(actor.VisibleArea).FromAffiliation(actor, AffiliationType.Enemy).Positions());
            return positions
                .OrderBy(p => Vector2Int.Distance(p, position))
                .Take(NumberOfTarget);
        }

        public float EvaluateHitProbability()
        {
            return NumberOfTarget;
        }

        public string UpgradePathName => "近くのキャラクター";
        public List<UpgradeData> GetUpgrades()
        {
            return new List<UpgradeData>
            {
                new UpgradeData(
                    "対象数+1",
                    () => NumberOfTarget += 1,
                    () => NumberOfTarget -= 1
                )
            };
        }
        public Dictionary<string, IHasUpgrades> GetChildren() => new();

        public string Info()
        {
            var info = "近くの";
            if (TargetAlly && TargetNeutral && TargetEnemy)
            {
                info += "キャラクター";
            }
            else
            {
                var targets = new List<string>();
                if (TargetAlly) targets.Add("味方");
                if (TargetNeutral) targets.Add("中立");
                if (TargetEnemy) targets.Add("敵");

                info += string.Join("、", targets);
            }

            if (TargetSelf) info += "（自分含む）";
            info += $"{NumberOfTarget}体";

            return info;
        }
    }
}