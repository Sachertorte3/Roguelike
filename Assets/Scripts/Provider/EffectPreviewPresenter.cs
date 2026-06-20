#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Item;
using Domain.Model.Setting;
using Domain.Service.Effect;
using Game;
using R3;
using UnityEngine;
using VContainer;
using View;
using View.UI;

namespace Provider
{
    public class EffectPreviewPresenter
    {
        [Inject]
        public EffectPreviewPresenter(GameManager gameManager, World world, EffectViewSpawner effectViewSpawner,
            InventoryView inventoryView)
        {
            var disposables = new CompositeDisposable();
            var previews = new List<GameObject>();
            world.OnActiveMapChanged.Subscribe(mapChanged =>
                {
                    disposables.Clear();
                    previews.ForEach(preview => Object.Destroy(preview));
                    previews.Clear();
                    var map = mapChanged.Map;
                    var player = map.Player.Character;
                    disposables.Add(map.OnEffectSpawned.Subscribe(effectSpawned =>
                        effectViewSpawner.Spawn(
                            effectSpawned.Area.Intersect(player.VisibleArea),
                            effectSpawned.Color,
                            Settings.GlobalSettings.EffectDisplayTime.CurrentValue
                        )
                    ));
                    disposables.Add(Observable.Merge(
                        inventoryView.Focus.AsUnitObservable(),
                        player.Inventory.OnItemRemoved.AsUnitObservable(),
                        player.Inventory.OnItemReplaced.AsUnitObservable(),
                        player.Direction.AsUnitObservable(),
                        gameManager.OnTurnChanged
                    ).Subscribe(_ =>
                    {
                        // 直前のプレビューを消してから、選択中アイテムの効果範囲を半透明で描き直す。
                        // 表示すべきでない条件はガード節で早期に抜け、ネストを浅く保つ。
                        previews.ForEach(preview => Object.Destroy(preview));
                        previews.Clear();
                        if (player.IsDead)
                            return;

                        var focus = inventoryView.Focus.CurrentValue;
                        if (focus.IsOnEmpty)
                            return;

                        // 足元のアイテムか、インベントリで選択中のアイテムかを取り出す。
                        var item = focus.IsOnGroundItem
                            ? map.Items.At(player.Entity.CurrentPosition).FirstOrDefault()?.Item
                            : player.Inventory.GetItem(focus.Index);
                        // 未識別アイテムは効果が分からないのでプレビューしない。
                        if (item == null || !player.IsKnownItem(item))
                            return;

                        // 効果範囲を持つスキル（SpawnEffectSkill）だけがプレビュー対象。
                        if (!item.SkillOnUse.HasValue)
                            return;
                        if (item.SkillOnUse.Value.Skill is not SpawnEffectSkill spawnEffectSkill)
                            return;

                        var area = spawnEffectSkill.GetArea(player, player.Entity.CurrentPosition,
                            player.CurrentDirection, map, true);
                        var color = spawnEffectSkill.Color;
                        color.a = 0.25f;
                        previews = effectViewSpawner.SpawnPreview(area, color);
                    }));
                }
            );
        }
    }
}