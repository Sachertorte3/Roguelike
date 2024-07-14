#nullable enable
using Model.Game;
using R3;
using Utilities;
using VContainer;
using View.UI;

namespace Provider
{
    public class ShopInfoPresenter
    {
        private CompositeDisposable _disposable = new();

        [Inject]
        public ShopInfoPresenter(World world, ShopInfoView shopInfoView)
        {
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
            {
                if (map.Shop != null)
                {
                    _disposable.Add(map.Shop.IsInside.Subscribe(isInside =>
                    {
                        shopInfoView.SetVisiblity(isInside);
                    }));
                    _disposable.Add(Observable.EveryUpdate().Subscribe(_ =>
                    {
                        shopInfoView.SetInfo(map.Player.Money, map.Shop.GetPurchasePrice(map), map.Shop.GetSalePrice(map));
                    }));
                }
            },
            _ => _disposable.Clear());
        }
    }
}