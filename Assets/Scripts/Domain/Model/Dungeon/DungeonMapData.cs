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
        FieldBluePrint Field,
        ITable<ItemData> Items,
        ITable<MaterialData> Materials,
        ITable<WeaponMold> WeaponMolds,
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
        float TrapChance,
        float ShopChance,
        float MonsterHouseChance,
        float RestRoomChance,
        float ItemCount,
        float WeaponCount,
        float CharacterCount,
        bool existBoss,
        List<EnemyData> Boss,
        EnemyData Clerk,
        EnemyData Mimic
    );
}