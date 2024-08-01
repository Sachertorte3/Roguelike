using System;
using Domain.Model.Character;

namespace Domain.Model.Map
{
    [Serializable]
    public class DownStairsMemento
    {
        public int DestinationMapId;
        public EntityMemento Entity;
    }
}