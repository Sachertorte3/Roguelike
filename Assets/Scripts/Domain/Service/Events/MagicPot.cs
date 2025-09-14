using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Items;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Domain.Service.Events
{
    public class MagicPot : IDisposable, ISerializable<MagicPotMemento>, IPlayerEventEntity, IIconEntity
    {
        private ReactiveProperty<int> _remainingUsages;
        public ReadOnlyReactiveProperty<bool> CanUse => _remainingUsages.Select(remainingUsages => remainingUsages > 0).ToReadOnlyReactiveProperty();
        public EntityBase Entity { get; init; }

        public MagicPot(MagicPotMemento data)
        {
            Entity = new EntityBase(data.Entity);
            _remainingUsages = new ReactiveProperty<int>(data.RemainingUsages);
            Event = new PlayerEvent(
                "魔法の壺を見つけた",
                new List<PlayerChoiceEvent>
                {
                    new(
                        "使う",
                        player => CanUse.CurrentValue,
                        async (gameManager, map) => await DoEvent(map)
                    )
                }
            );
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public Sprite Icon => Addressables.LoadAssetAsync<Sprite>($"Assets/Images/icons_full_16.png[icons_full_16_{(CanUse.CurrentValue ? 270 : 269)}]")
            .WaitForCompletion();

        public IPlayerEvent Event { get; init; }

        private async UniTask DoEvent(IMap map)
        {
            var player = map.Player;
            var mergeBaseItem = await player.Character.ItemSelector.SelectItemWithCanSelect(
                "ベースのアイテムを選択してください",
                player,
                map,
                ItemMergeExtension.CanSelectForBaseItem) as DirectWeapon;
            if (mergeBaseItem == null)
                return;
            var mergedItem = await player.Character.ItemSelector.SelectItemWithCanSelect(
                "合成するアイテムを選択してください",
                player,
                map,
                item => ItemMergeExtension.CanSelectForMergedItem(item as BaseItem, mergeBaseItem));
            if (mergedItem == null)
                return;

            player.Character.Inventory.Remove(mergeBaseItem);
            player.Character.Inventory.Remove(mergedItem);
            player.Character.Inventory.Add(mergeBaseItem.Merge(mergedItem));
            GameLog.AddIgnoreVisibility($"{player.Character.GetName(player)}は{mergeBaseItem.GetName(player, map.ItemPlaceholders)}と{mergedItem.GetName(player, map.ItemPlaceholders)}を合成した。");
            _remainingUsages.Value -= 1;
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public MagicPotMemento Serialize()
        {
            return new MagicPotMemento(_remainingUsages.CurrentValue, Entity.Serialize());
        }

        public static MagicPotMemento Build(Vector2Int position)
        {
            return new MagicPotMemento(3, EntityBase.Build(position, EntityLayer.Middle));
        }

        ~MagicPot()
        {
            Dispose();
        }
    }
}