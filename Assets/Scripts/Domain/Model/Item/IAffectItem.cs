#nullable enable
namespace Domain.Model.Item
{
    public interface IItemEffect : IHasInfo
    {
        void Apply(IItem item);
    }
}