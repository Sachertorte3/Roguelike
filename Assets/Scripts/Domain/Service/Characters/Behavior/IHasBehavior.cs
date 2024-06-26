using Domain.Service.Action;
using Domain.Service.Effect;
using Domain.Service.Items;

namespace Domain.Service.Characters.Behavior
{
    public interface IHasBehavior : IActor
    {
        public Skill[] Skills { get; }
        public IInventory Inventory { get; }
        public IVisionRange Area { get; }
    }
}