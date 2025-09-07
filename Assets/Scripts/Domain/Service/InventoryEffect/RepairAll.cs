using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;

namespace Domain.Service.InventoryEffect
{
    public class RepairAll : IInventoryEffect
    {
        public void Apply(IPlayer player, IStorage storage, ItemPlaceholders itemPlaceholders)
        {
            foreach (var item in storage.AllItems)
            {
                item.Repair(player, itemPlaceholders);
            }
        }

        public float EvaluatePrice()
        {
            return 500 * 5;
        }

        public string Info()
        {
            return "修理(全て)";
        }
    }
    public class CurseAll : IInventoryEffect
    {
        public void Apply(IPlayer player, IStorage storage, ItemPlaceholders itemPlaceholders)
        {
            foreach (var item in storage.AllItems)
            {
                item.SetCursed(player, itemPlaceholders, true);
            }
        }

        public float EvaluatePrice()
        {
            return 100 * 5;
        }

        public string Info()
        {
            return "呪い(全て)";
        }
    }
}