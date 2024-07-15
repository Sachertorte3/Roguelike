using Domain.Model.Action;
using Domain.Model.Effect;
using Domain.Model.Item;

namespace Domain.Model.Character
{
    public interface IHasBehavior : IActor
    {
        public ISkill[] Skills { get; }
        public IInventory Inventory { get; }
        public IVisionRange VisionRange { get; }
    }
}