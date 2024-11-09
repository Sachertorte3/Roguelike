using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;

namespace Domain.Service.ItemEffect
{
    public class UnleashCurse : IItemEffect
    {
        public bool CanApplyTo(IPlayer player, IItem item)
        {
            return item.IsCursed || (!player.Character.IsKnownItem(item) && !item.IsCurseIdentified);
        }

        public void Apply(IPlayer player, IItem item, ItemPlaceholders itemPlaceholders)
        {
            item.SetCursed(player, itemPlaceholders, false);
        }

        public float EvaluatePrice()
        {
            return 200;
        }

        public string Info()
        {
            return "解呪";
        }
    }
}