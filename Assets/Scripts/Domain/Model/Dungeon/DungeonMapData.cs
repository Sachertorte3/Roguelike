using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using RandomDungeonWithBluePrint;
using UnityEngine;
using Utilities;
using Utilities.Table;

namespace Domain.Model.Dungeon
{
    public record DungeonMapData(
        string Name,
        int Depth,
        float Progress,
        MapType Type,
        FieldBluePrint Field,
        ItemDatabase ItemDatabase,
        ICorrectionTable<WeaponPrefix> WeaponPrefixes,
        ICorrectionTable<IItemData> ChestItems,
        ITable<TrapData> Traps,
        ITable<StatueData> Statues,
        ITable<ShopItemData> ShopItems,
        Table<EnemyData> Enemies,
        Table<EnemyData> Npcs,
        float PrefixChance,
        float ShinyChance,
        float SleepChance,
        float MimicChance,
        float WeaponChanceInChest,
        bool RoundRoomCorner,
        bool CaveInOneRoom,
        float WaterChance,
        float GrassChance,
        float ShopChance,
        float MonsterHouseChance,
        float RestRoomChance,
        float LakeChance,
        float ItemAttempt,
        float MoneyAttempt,
        float MoneyAverage,
        float CharacterAttempt,
        float TrapAttempt,
        float StatueChance,
        float BonfireWeight,
        float MagicPotWeight,
        float WorkbenchWeight,
        List<EnemyData> Boss,
        EnemyData Clerk,
        EnemyData Mimic
    )
    {
        public bool ExistBoss => Boss.Count > 0;
        private int GetCount(float trials)
        {
            return RandUtils.Binomial(trials * 2, 0.5f);
        }

        public int ItemCount()
        {
            return GetCount(ItemAttempt);
        }

        public int MoneyCount()
        {
            return GetCount(MoneyAttempt);
        }

        public int CharacterCount()
        {
            return GetCount(CharacterAttempt);
        }

        public int TrapCount()
        {
            return GetCount(TrapAttempt);
        }

        public int MoneyAmount()
        {
            return Mathf.CeilToInt(RandUtils.LogNormalFromMean(MoneyAverage, 1));
        }
    }
}