using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Item;
using RandomDungeonWithBluePrint;

namespace Domain.Model
{
    public record DungeonMapData(
        string Name,
        FieldBluePrint Field,
        ITable<ItemData> Items,
        ITable<MaterialData> Materials,
        ITable<WeaponMold> WeaponMolds,
        ITable<WeaponPrefix> WeaponPrefixes,
        ITable<ItemData> ChestItems,
        ITable<ShopItemData> ShopItems,
        Table<EnemyData> Enemies,
        float PrefixChance,
        float ShinyChance,
        float SleepChance,
        float ChestChance,
        float WeaponChanceInChest,
        float ShopChance,
        float MonsterHouseChance,
        float ItemCount,
        float WeaponCount,
        float CharacterCount,
        bool existBoss,
        List<EnemyData> Boss,
        EnemyData Clerk
    );
}