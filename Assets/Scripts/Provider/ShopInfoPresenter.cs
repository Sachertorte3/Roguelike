#nullable enable
using Game;
using R3;
using Utilities;
using VContainer;
using View.UI;

namespace Provider
{
    public class ShopInfoPresenter
    {
        [Inject]
        public ShopInfoPresenter(World world, ShopInfoView shopInfoView)
        {
            var disposable = new CompositeDisposable();
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
                {
                    if (map.Shop != null)
                    {
                        disposable.Add(map.Shop.IsInside.SubscribeToAll(isInside =>
                        {
                            shopInfoView.SetVisibility(isInside);
                        }));
                        disposable.Add(Observable.EveryUpdate().Subscribe(_ =>
                        {
                            shopInfoView.SetInfo(map.Player.Money, map.Shop.GetPurchasePrice(map),
                                map.Shop.GetSalePrice(map));
                        }));
                    }
                },
                _ => disposable.Clear());
        }
    }
}