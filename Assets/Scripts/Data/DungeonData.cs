using System.Collections.Generic;
using Data.Character;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Dungeon")]
    public class DungeonData : ScriptableObject
    {
        public int Floor;
        public List<ItemData> Items;
        public List<EnemyData> Enemies;
    }
}