using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using RandomDungeonWithBluePrint;
using UnityEngine;
using Utilities;
using Utilities.Table;

namespace Domain.Model.Dungeon
{
    public record DungeonMapData(
        string Name,
        SectionType Type,
        FieldBluePrint Field,
        ItemDatabase ItemDatabase,
        ITable<WeaponPrefix> WeaponPrefixes,
        ITable<ItemData> ChestItems,
        ITable<TrapData> Traps,
        ITable<ShopItemData> ShopItems,
        Table<EnemyData> Enemies,
        Table<EnemyData> Npcs,
        float PrefixChance,
        float ShinyChance,
        float SleepChance,
        float ChestChance,
        float MimicChance,
        float WeaponChanceInChest,
        bool RoundRoomCorner,
        bool CaveInOneRoom,
        float WaterChance,
        float GrassChance,
        float ShopChance,
        float MonsterHouseChance,
        float RestRoomChance,
        float ItemAttempt,
        float MoneyAttempt,
        float MoneyAverage,
        float CharacterAttempt,
        float TrapAttempt,
        bool ExistBoss,
        List<EnemyData> Boss,
        EnemyData Clerk,
        EnemyData Mimic
    )
    {
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