using Model.Action;
using Model.Items;

namespace Model.Characters.Behavior
{
    public interface IHasBehavior : IActor
    {
        public IInventory Inventory { get; }
        public IVisionRange Area { get; }
    }
}