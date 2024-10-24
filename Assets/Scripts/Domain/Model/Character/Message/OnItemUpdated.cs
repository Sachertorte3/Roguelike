using Domain.Model.Item;

namespace Domain.Model.Message
{
    public record OnItemUpdated(IItem Item, int Index);
    public record OnCursedChanged(bool IsCursed, int Index);
}