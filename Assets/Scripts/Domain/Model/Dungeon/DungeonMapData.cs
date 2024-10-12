using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Item;
using RandomDungeonWithBluePrint;
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
        float ItemCount,
        float CharacterCount,
        float TrapCount,
        bool ExistBoss,
        List<EnemyData> Boss,
        EnemyData Clerk,
        EnemyData Mimic
    );
}