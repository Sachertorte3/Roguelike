using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Effect;
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
        private int GetCount(float trials) => RandUtils.Binomial(trials * 2, 0.5f);
        public int ItemCount() => GetCount(ItemAttempt);
        public int MoneyCount() => GetCount(MoneyAttempt);
        public int CharacterCount() => GetCount(CharacterAttempt);
        public int TrapCount() => GetCount(TrapAttempt);
        public int MoneyAmount() => Mathf.CeilToInt(RandUtils.LogNormalFromMean(MoneyAverage, MoneyAverage / 100f));
    }
}