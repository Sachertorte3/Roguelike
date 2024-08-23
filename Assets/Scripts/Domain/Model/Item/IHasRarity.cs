#nullable enable
namespace Domain.Model.Item
{
    public interface IHasRarity
    {
        Rarity Rarity { get; }
    }
}