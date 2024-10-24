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
        [MinValue(1), SerializeField] private int _power;
        public Element Element => _element;
        public int Power => _power;

        public ElementPower(Element element, int power)
        {
            _element = element;
            _power = power;
        }

        public void Upgrade(int value)
        {
            _power += value;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() => new Dictionary<UpgradePath, UpgradeData>
        {
            {
                new UpgradePath("威力[小]", Element.ToString()),
                new UpgradeData($"[{Element}]威力[小]", () => Upgrade(2))
            },
            {
                new UpgradePath("威力[大]", Element.ToString()),
                new UpgradeData($"[{Element}]威力[大]", () => Upgrade(3))
            }
        };

        public string Info()
        {
            return $"[{Element}] {Power}";
        }
    }
}