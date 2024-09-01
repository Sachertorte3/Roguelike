#nullable enable
using Domain.Service.Events;
using R3;

namespace Model.Game
{
    public interface IShop
    {
        public ReadOnlyReactiveProperty<bool> IsInside { get; }
        public int GetPurchasePrice(IMapManager mapManager);
        public int GetSalePrice(IMapManager mapManager);
    }
}