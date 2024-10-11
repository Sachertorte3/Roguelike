using System;
using System.Collections.Generic;
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
    public class CurseItemEffect : IEffect
    {
        [OnInspectorInit("OnProbabilityOfSuccessChanged")]
        [SerializeField]
        [Range(0, 1)]
        private float _probabilityOfSuccess = 0.25f;

        public Color Color => Colors.MediumPurple;

        public Impact Impact => Impact.Harmful;
#if UNITY_EDITOR
        private void OnProbabilityOfSuccessChanged()
        {
            if (_probabilityOfSuccess == 0)
                _probabilityOfSuccess = 0.25f;
        }
#endif
        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IMap map)
        {
            var notCursedItems = target.Inventory.AllItems.Where(item => !item.IsCursed).ToArray();
            if (notCursedItems.Any())
            {
                var item = notCursedItems.GetAtRandom();
                if (Random.value < _probabilityOfSuccess)
                    item.SetCursed(true);
                else
                    GameLog.Add($"{item.Name}は呪われなかった");
            }
            else
            {
                GameLog.Add($"{target.GetName(map.Player)}は呪いの対象になるアイテムを持っていない");
            }
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0.25f;
        }

        public float EvaluatePrice()
        {
            return 100;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            return new Dictionary<UpgradePath, UpgradeData>();
        }

        public string Info()
        {
            return $"呪い";
        }
    }
}