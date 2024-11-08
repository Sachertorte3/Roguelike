using System;
using System.Collections.Generic;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Item;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Service.Effect
{
    [Serializable]
    public class ElementPower : IHasInfo, IHasUpgrades
    {
        [SerializeField] private Element _element;
        [MinValue(1)][SerializeField] private int _power;
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

        public string UpgradePathName => $"[{Element}]威力";
        public List<UpgradeData> GetUpgrades()
        {
            return new List<UpgradeData>
            {
                new UpgradeData(
                    $"[{Element}]強化[小]",
                    () => _power += 2,
                    () => _power -= 2
                ),
                new UpgradeData(
                    $"[{Element}]強化[大]",
                    () => _power += 3,
                    () => _power -= 3
                )
            };
        }
        public Dictionary<string, IHasUpgrades> GetChildren() => new();

        public string Info()
        {
            return $"{Element.Name()}属性、威力{Power}";
        }
    }
}