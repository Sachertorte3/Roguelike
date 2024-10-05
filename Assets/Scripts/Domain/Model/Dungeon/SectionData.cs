using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Item;
using Sirenix.OdinInspector;
using Utilities.Table;

namespace Domain.Model.Dungeon
{
    [Serializable]
    public class SectionData
    {
        public int Depth => Floors.Sum(floor => floor.Depth);
        public SectionType Type;
        [RequiredListLength(1, null)] public List<FloorData> Floors;
        private bool _existChest => Floors.Any(floor => floor.Room.ChestChance > 0);
        [ShowIf("@_existChest"), Required] public float WeaponChanceInChest;
        private bool _existMimic => Floors.Any(floor => floor.MimicChance > 0);
        [ShowIf("@_existMimic"), Required] public EnemyData Mimic;
        private bool _existShop => Floors.Any(floor => floor.ShopChance > 0);
        [ShowIf("@_existShop"), Required] public EnemyData Clerk;
        public Table<EnemyData> Enemies;
        public Table<MaterialData> Materials;
    }
}