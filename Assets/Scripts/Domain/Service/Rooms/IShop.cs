#nullable enable
using Domain.Model.Map;
using R3;

namespace Domain.Service.Rooms
{
    public interface IShop
    {
        public ReadOnlyReactiveProperty<bool> IsInside { get; }
        public int GetPurchasePrice(IMapManager mapManager);
        public int GetSalePrice(IMapManager mapManager);
    }
}