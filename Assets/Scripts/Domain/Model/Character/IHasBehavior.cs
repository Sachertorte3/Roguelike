using System.Collections.Generic;
using Domain.Model.Action;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Character
{
    public interface IHasBehavior : IActor
    {
        public bool CanPickUp { get; }
        public bool CanUseItem { get; }
        public IReadOnlyList<ICharacterSkill> Skills { get; }
        public IInventory Inventory { get; }
        public IVisionRange VisionRange { get; }
        public int CurrentHp { get; }
        public bool CanSwap(Vector2Int position, Direction8 direction, IMap map);
        public bool TryPickUpItem(IMap map, bool canPickUpShopItem);
    }
}