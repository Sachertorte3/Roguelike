using System.Collections.Generic;
using Data.Character;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Dungeon")]
    public class DungeonData : ScriptableObject
    {
        public int Floor;
        [RequiredListLength(1, null)] public List<ItemData> Items;
        [RequiredListLength(1, null)] public List<EnemyData> Enemies;
        [RequiredListLength(1, null)] public List<MaterialData> Materials;
        [RequiredListLength(1, null)] public List<WeaponMold> WeaponMolds;
    }
}

