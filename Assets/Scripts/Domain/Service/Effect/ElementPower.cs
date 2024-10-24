using System;
using System.Collections.Generic;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Service.Effect
{
    [Serializable]
    public class ElementPower : IHasInfo, IHasUpgrades
    {
        [SerializeField] private Element _element;
        [MinValue(1)] [SerializeField] private int _power;
        public Element Element => _element;
        public int Power => _power;

        public ElementPower(Element element, int power)
        {
            _element = element;
            _power = power;
        }

        public void MultiplyPower(float multiplier)
        {
            _power = Mathf.RoundToInt(_power * multiplier);
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            return new Dictionary<UpgradePath, UpgradeData>
            {
                {
                    new UpgradePath("威力[小]", Element.ToString()),
                    new UpgradeData(
                        $"[{Element}]威力[小]",
                        () => _power += 2,
                        () => _power -= 2
                    )
                },
                {
                    new UpgradePath("威力[大]", Element.ToString()),
                    new UpgradeData(
                        $"[{Element}]威力[大]",
                        () => _power += 3,
                        () => _power -= 3
                    )
                }
            };
        }

        public string Info()
        {
            return $"[{Element}] {Power}";
        }
    }
}