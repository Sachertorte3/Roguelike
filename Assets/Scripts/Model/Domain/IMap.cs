using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Model.Domain.Characters;
using Model.Domain.Items;
using UnityEngine;
using ObservableCollections;

namespace Model.Domain
{
    public interface IMap
    {
        public IObservableCollection<Vector2Int> VisibleArea { get; }
        public IObservableCollection<Character> Characters { get; }
        public IObservableCollection<ItemEntity> Items { get; }
        public HashSet<Character> GetCharactersInArea(HashSet<Vector2Int> area);
        public HashSet<Vector2Int> GetAllLightPassablePositions();
        public bool IsPassable(Vector2Int position);
        public bool IsMapPassable(Vector2Int position);
        public bool IsReachable(Vector2Int from, Vector2Int to);
        public ItemEntity SpawnItem(Item item, Vector2Int position);
    }
}