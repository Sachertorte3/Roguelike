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
            world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(
                map =>
                {
                    disposables.Add(map.OnEffectSpawned.Subscribe(effectSpawned =>
                        effectViewSpawner.Spawn(
                            effectSpawned.Area.Intersect(map.Player.Character.VisibleArea),
                            effectSpawned.Color,
                            Settings.EffectDisplayTime.Value
                        )
                    ));
                    disposables.Add(Observable.Merge(
                        inventoryView.Focus.AsUnitObservable(),
                        map.Player.Character.Inventory.OnItemChanged.AsUnitObservable(),
                        map.Player.Character.Direction.AsUnitObservable(),
                        gameManager.OnTurnChanged
                    ).Subscribe(_ =>
                    {
                        previews.ForEach(preview => Object.Destroy(preview));
                        previews.Clear();
                        if (map.Player.Character.IsDead)
                        {
                            return;
                        }

                        var focus = inventoryView.Focus.CurrentValue;
                        if (!focus.IsEmpty)
                        {
                            IItem? item = null;
                            if (focus.IsGroundItem)
                            {
                                item = map.Items.At(map.Player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
                            }
                            else
                            {
                                item = map.Player.Character.Inventory.GetItem(focus.Index);
                            }

                            if (item != null && map.Player.Character.IsKnownItem(item))
                            {
                                if (item.SkillOnUse.HasValue &&
                                    item.SkillOnUse.Value is SpawnEffectSkill spawnEffectSkill)
                                {
                                    var area = spawnEffectSkill.GetArea(map.Player.Character,
                                        map.Player.Character.Entity.CurrentPosition,
                                        map.Player.Character.CurrentDirection, map, true);
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