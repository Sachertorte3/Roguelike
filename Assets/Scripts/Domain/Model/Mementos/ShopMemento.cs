using System.Collections.Generic;
using Domain.Model.Character;

namespace Domain.Model.Map
{
    public record ShopMemento(
        RoomMemento Room,
        EntityMemento Clerk,
        List<ItemEntityMemento> Items
    );
}