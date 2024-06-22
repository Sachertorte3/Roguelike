#nullable enable
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Model.Domain.Events;
using ObservableCollections;
using UnityEngine;

namespace Model.Game
{
    public class EventEntityManager
    {
        public UpStairs? UpStairs { get; init; }
        public DownStairs DownStairs { get; init; }
        private readonly List<Chest> _chests = new();
        public ReadOnlyCollection<Chest> Chests => new(_chests); 
        private ObservableList<IEventEntity> _eventEntities = new();
        public IObservableCollection<IEventEntity> EventEntities => _eventEntities;
        public EventEntityManager(DownStairs downStairs, UpStairs? upStairs=null)
        {
            DownStairs = downStairs;
            UpStairs = upStairs;

            _eventEntities.Add(downStairs);
            if (upStairs != null)
            {
                _eventEntities.Add(upStairs);
            }
        }
        public void Add(Chest chest)
        {
            _chests.Add(chest);
            _eventEntities.Add(chest);
        }
        public void Remove(Chest chest)
        {
            _chests.Remove(chest);
            _eventEntities.Remove(chest);
        }
    }
}