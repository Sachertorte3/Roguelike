using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Service.Logs;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Domain.Service.Effect
{
    [Serializable]
    public class RemoveUpgradeEffect : ActorlessEntityTargetEffect
    {
        [OnInspectorInit("OnProbabilityOfSuccessChanged")]
        [SerializeField]
        [Range(0, 1)]
        private float _probabilityOfSuccess = 0.1f;

        public override Color Color => Colors.SandyBrown;
        public override Impact Impact => Impact.Harmful;
#if UNITY_EDITOR
        private void OnProbabilityOfSuccessChanged()
        {
            if (_probabilityOfSuccess == 0)
                _probabilityOfSuccess = 0.1f;
        }
#endif
        public override UniTask Apply(ITargetOfEffect target, Vector2Int position, IMap map)
        {
            var upgradedItems = target.Inventory.AllItems.Where(item => item.AppliedUpgrades > 0).ToArray();
            if (upgradedItems.Any())
            {
                var item = upgradedItems.GetAtRandom();
                if (Random.value < _probabilityOfSuccess)
                    item.Downgrade(map.Player, map.ItemPlaceholders);
                else
                    GameLog.Add($"{item.GetName(map.Player, map.ItemPlaceholders)}の強化は消えなかった");
            }
            else
            {
                GameLog.Add($"{target.GetName(map.Player)}は強化されたアイテムを持っていない");
            }
            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0.25f;
        }

        public override float EvaluatePrice()
        {
            return 100;
        }

        public override string Info()
        {
            return $"強化解除";
        }
    }
}