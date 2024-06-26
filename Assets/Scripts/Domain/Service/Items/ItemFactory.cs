#nullable enable
using Domain.Model.Map;

namespace Model.Domain.Items
{
    public sealed class ItemFactory
    {
        public ItemEntity CreateItem(ItemEntityMemento item)
        {
            return new ItemEntity(item);
        }
    }
}