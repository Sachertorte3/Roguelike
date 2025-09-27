#nullable enable
using System.Collections.Generic;
using Domain.Model.Item;

namespace Domain.Model.Character.Message
{
    public record OnItemChanged(IItem? OldItem, IItem? NewItem, int Index);
    public record OnItemUpdated(IItem Item, int Index);
    public record OnItemOverflowed(IItem From, IEnumerable<IItem> Items);
}