using Domain.Model.Item;

namespace Domain.Model.Character.Message
{
    public record OnItemUpdated(IItem Item, int Index);
}