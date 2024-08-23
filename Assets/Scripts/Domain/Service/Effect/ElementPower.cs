using System;
using Domain.Model;
using Domain.Model.Character;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Service.Effect
{
    [Serializable]
    public class ElementPower : IHasInfo
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

        public string Info()
        {
            return $"[{Element}] {Power}";
        }
    }
}