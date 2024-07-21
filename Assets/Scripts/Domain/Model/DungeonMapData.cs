using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Item;
using RandomDungeonWithBluePrint;

namespace Domain.Model
{
    public record DungeonMapData(
        FieldBluePrint Field,
        RarityWeightTable<ItemData> Items,
        Table<MaterialData> Materials,
        Table<WeaponMold> WeaponMolds,
        RarityWeightTable<WeaponPrefix> WeaponPrefixes,
        RarityWeightTable<ItemData> ChestItems,
        RarityWeightTable<ItemData> ShopItems,
        Table<EnemyData> Enemies,
        float PrefixChance,
        float ShinyChance,
        float SleepChance,
        float ChestChance,
        float WeaponChanceInChest,
        float ShopChance,
        float MonsterHouseChance,
        int ItemCount,
        int WeaponCount,
        int CharacterCount,
        bool existBoss,
        List<EnemyData> Boss,
        EnemyData Clerk
    );
}