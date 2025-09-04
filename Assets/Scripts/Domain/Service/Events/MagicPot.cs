using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Items;
using Domain.Service.Logs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Domain.Service.Events
{
    public class MagicPot : IDisposable, ISerializable<EntityMemento>, IPlayerEventEntity, IIconEntity
    {
        public EntityBase Entity { get; init; }

        public MagicPot(EntityMemento data)
        {
            Entity = new EntityBase(data);
            Event = new PlayerEvent(
                "魔法の壺を見つけた",
                true,
                new List<PlayerChoiceEvent>
                {
                    new(
                        "使う",
                        player => true,
                        async (gameManager, map) => await DoEvent(map)
                    )
                }
            );
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public Sprite Icon => Addressables.LoadAssetAsync<Sprite>("Assets/Images/icons_full_16.png[icons_full_16_270]")
            .WaitForCompletion();

        public IPlayerEvent Event { get; init; }

        private bool canSelectForBaseItem(IItem baseItem) => baseItem is DirectWeapon;
        private bool canSelectForMergedItem(BaseItem baseItem, DirectWeapon mergeBaseItem)
        {
            if (baseItem == mergeBaseItem)
                return false;
            var featuresToMergeWeapon = baseItem switch
            {
                DirectWeapon weapon => weapon.Features,
                Item item => item.FeaturesToMergeWeapon,
                _ => throw new Exception("Invalid item")
            };
            var mergeBaseItemFeatures = mergeBaseItem.Features;
            if (!mergeBaseItemFeatures.Merge(featuresToMergeWeapon).SequenceEqual(mergeBaseItemFeatures))
            {
                return true;
            }
            foreach (var upgradePath in baseItem.UpgradePaths)
            {
                if (mergeBaseItem.CanUpgrade(upgradePath.ToString()))
                {
                    return true;
                }
            }
            return false;
        }

        private async UniTask DoEvent(IMap map)
        {
            var player = map.Player;
            var mergeBaseItem = await player.Character.ItemSelector.SelectItemWithCanSelect("ベースのアイテムを選択してください", player, map, canSelectForBaseItem) as DirectWeapon;
            if (mergeBaseItem == null)
                return;
            var mergedItem = await player.Character.ItemSelector.SelectItemWithCanSelect("合成するアイテムを選択してください", player, map, item => canSelectForMergedItem(item as BaseItem, mergeBaseItem));
            if (mergedItem == null)
                return;

            player.Character.Inventory.Remove(mergeBaseItem);
            player.Character.Inventory.Remove(mergedItem);
            player.Character.Inventory.Add(mergeBaseItem.Merge(mergedItem));
            GameLog.Add($"{player.Character.GetName(player)}は{mergeBaseItem.GetName(player, map.ItemPlaceholders)}と{mergedItem.GetName(player, map.ItemPlaceholders)}を合成した。");
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public EntityMemento Serialize()
        {
            return Entity.Serialize();
        }

        public static EntityMemento Build(Vector2Int position)
        {
            return EntityBase.Build(position, EntityLayer.Middle);
        }

        ~MagicPot()
        {
            Dispose();
        }
    }
}