using Domain.Model.Action;
using Domain.Model.Items;
using Domain.Model.Action;
using Domain.Model.Characters;
using Domain.Model.Character;

namespace Domain.Model.Characters
{
    public interface IHasBehavior : IActor
    {
        public ISkill[] Skills { get; }
        public IInventory Inventory { get; }
        public IVisionRange VisionRange { get; }
    }
}