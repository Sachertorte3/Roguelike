#nullable enable
namespace Domain.Model.Character
{
    public record OnItemSelectMessage(bool IsWaiting, int[] DisabledItemIds);
}