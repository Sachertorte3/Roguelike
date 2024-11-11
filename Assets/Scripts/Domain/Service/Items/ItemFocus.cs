using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Service.Items
{
    public record ItemFocus(int index, bool isGroundItem, bool isEmpty)
    {
        public IItem? GetItem(IStorage storage, IMap map)
        {
            if (isEmpty)
                return null;
            if (isGroundItem)
                return map.Items.At(map.Player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
            return storage.GetItem(index);
        }
    }
}