using Domain.Model.Item;

namespace Domain.Model.Character
{
    public record OnStartItemSelectMessage(string Text, ItemFocus[] DisabledItemIndexes);
}