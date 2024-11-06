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
using Utilities;
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
            world.ActiveMap.SubscribeToAllItemsIgnoreNull(
                map =>
                {
                    disposables.Add(map.OnEffectSpawned.Subscribe(effectSpawned =>
                        effectViewSpawner.Spawn(
                            effectSpawned.Area.Intersect(map.VisibleArea),
                            effectSpawned.Color,
                            Settings.EffectDisplayTime.Value
                        )
                    ));
                    disposables.Add(Observable.Merge(
                        inventoryView.OnFocusChanged.AsUnitObservable(),
                        map.Player.Inventory.OnItemChanged.AsUnitObservable(),
                        map.Player.Direction.AsUnitObservable(),
                        gameManager.Turn.AsUnitObservable()
                    ).Subscribe(_ =>
                    {
                        previews.ForEach(preview => Object.Destroy(preview));
                        previews.Clear();
                        if (map.Player.CurrentHp <= 0)
                        {
                            return;
                        }

                        var focus = inventoryView.CurrentFocus;
                        if (!focus.isEmpty)
                        {
                            IItem? item = null;
                            if (focus.isGroundItem)
                            {
                                item = map.Items.At(map.Player.Entity.CurrentPosition).FirstOrDefault()?.Item;
                            }
                            else
                            {
                                item = map.Player.Inventory.GetItem(focus.index);
                            }
                            if (item != null && map.Player.IsKnownItem(item))
                            {
                                if (item.SkillOnUse.HasValue && item.SkillOnUse.Value is SpawnEffectSkill spawnEffectSkill)
                                {
                                    var area = spawnEffectSkill.GetArea(map.Player, map.Player.Entity.CurrentPosition,
                                        map.Player.CurrentDirection, map, true);
                                    var color = spawnEffectSkill.Color;
                                    color.a = 0.25f;
                                    previews = effectViewSpawner.SpawnPreview(area, color);
                                }
                            }
                        }
                    }));
                },
                _ => disposables.Clear()
            );
        }
    }
}