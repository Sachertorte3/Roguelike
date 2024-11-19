using System;
using Domain.Model.Item;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ItemTargetSkillMemento : ISkillMemento
    {
        [field: SerializeReference] public IItemEffect ItemEffect { get; private set; }

        public ItemTargetSkillMemento(IItemEffect itemEffect)
        {
            ItemEffect = itemEffect;
        }
    }
}