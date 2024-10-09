#nullable enable
using System.Collections.Generic;
using System.Linq;
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
            world.ActiveMap.SubscribeToAllIgnoreNull(
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
                        if (focus != null)
                        {
                            var item = map.Player.Inventory.GetItem(focus.Value);
                            if (item != null)
                            {
                                if (item.SkillOnUse.HasValue && item.SkillOnUse.Value is SpawnEffectSkill spawnEffectSkill)
                                {
                                    var area = spawnEffectSkill.GetArea(map.Player, map.Player.CurrentPosition,
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