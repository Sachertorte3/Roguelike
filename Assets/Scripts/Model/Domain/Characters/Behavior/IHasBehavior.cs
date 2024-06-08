using Model.Domain.Action;
using Model.Domain.Effect;
using Model.Domain.Items;

namespace Model.Domain.Characters.Behavior
{
    public interface IHasBehavior : IActor
    {
        public Skill Skill { get; }
        public IInventory Inventory { get; }
        public IVisionRange Area { get; }
    }
}