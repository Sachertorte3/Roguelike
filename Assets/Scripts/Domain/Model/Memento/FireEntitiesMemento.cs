using System;
using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class FireEntitiesMemento
    {
        [field: SerializeField] public List<EntityMemento> Fires { get; private set; }

        public FireEntitiesMemento(List<EntityMemento> fires)
        {
            Fires = fires;
        }
    }
}