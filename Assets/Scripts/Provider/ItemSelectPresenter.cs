#nullable enable
using Model.Game;
using R3;
using Utilities;
using VContainer;
using View.UI;

namespace Provider
{
    public class ItemSelectPresenter
    {
        [Inject]
        public ItemSelectPresenter(World world, ItemSelectText itemSelectText)
        {
            var disposable = new SerialDisposable();
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
            {
                disposable.Disposable = map.Player.IsWaitingItemSelect.Subscribe(isWaiting =>
                {
                    if (isWaiting)
                    {
                        itemSelectText.Show();
                    }
                    else
                    {
                        itemSelectText.Hide();
                    }
                });
            });
        }
    }
}