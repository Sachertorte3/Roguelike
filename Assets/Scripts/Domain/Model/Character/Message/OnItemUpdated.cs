using Domain.Model.Items;

namespace Domain.Model.Message
{
    public record OnItemUpdated(IItem Item, int Index);
}