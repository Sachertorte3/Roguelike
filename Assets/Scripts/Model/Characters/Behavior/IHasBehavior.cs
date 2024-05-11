using Assets.Scripts.Model.Items;
using Scripts.Model.Action;

namespace Scripts.Model.Characters.Behavior
{
    internal interface IHasBehavior : IActor
    {
        public IInventory Inventory { get; }
        public IVisionRange Area { get; }
    }
}
