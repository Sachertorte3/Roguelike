using Domain.Model.Item;

namespace Domain.Model.Character
{
    public record OnItemSelectMessage(bool IsWaiting, ItemFocus[] DisabledItemIndexes);
}