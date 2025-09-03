using Domain.Model.Item;

namespace Domain.Model.Character
{
    public record OnItemSelectMessage(string Text, bool IsWaiting, ItemFocus[] DisabledItemIndexes);
}