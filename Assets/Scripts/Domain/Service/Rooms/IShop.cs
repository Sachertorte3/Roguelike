#nullable enable
using Domain.Model.Map;
using R3;

namespace Domain.Service.Rooms
{
    public interface IShop
    {
        public ReadOnlyReactiveProperty<bool> IsInside { get; }
        public int GetPurchasePrice(IMap mapManager);
        public int GetSalePrice(IMap mapManager);
    }
}