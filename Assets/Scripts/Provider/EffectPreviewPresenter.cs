#nullable enable
using System.Collections.Generic;
using Domain.Service.Effect;
using Model.Game;
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
        public EffectPreviewPresenter(GameManager gameManager, World world, EffectViewSpawner effectViewSpawner, InventoryView inventoryView)
        {
            var serialDisposable = new SerialDisposable();
            var previews = new List<GameObject>();
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
            {
                serialDisposable.Disposable = Observable.Merge(
                    inventoryView.OnFocusChanged.AsUnitObservable(),
                    map.Player.Inventory.OnItemChanged.AsUnitObservable(),
                    gameManager.Turn.AsUnitObservable()
                ).Subscribe(_ =>
                {
                    if (map.Player.CurrentHp <= 0)
                    {
                        return;
                    }
                    var focus = inventoryView.CurrentFocus;
                    previews.ForEach(preview => GameObject.Destroy(preview));
                    previews.Clear();
                    if (focus != null)
                    {
                        var item = map.Player.Inventory.GetItem(focus.Value);
                        if (item != null)
                        {
                            if (item.SkillOnUse.HasValue && item.SkillOnUse.Value is SpawnEffectSkill spawnEffectSkill)
                            {
                                var area = spawnEffectSkill.GetArea(map.Player, map.Player.CurrentPosition, map.Player.CurrentDirection, map);
                                var color = spawnEffectSkill.Color;
                                color.a = 0.25f;
                                previews = effectViewSpawner.SpawnPreview(area, color);
                            }
                        }
                    }
                });
            });
        }
    }
}