using System;
using Domain.Model.Character;

namespace Domain.Model.Map
{
    [Serializable]
    public class UpStairsMemento
    {
        public int DestinationMapId;
        public EntityMemento Entity;
    }
}