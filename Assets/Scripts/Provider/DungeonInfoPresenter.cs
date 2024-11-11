#nullable enable
using Game;
using Utilities;
using VContainer;
using View.UI;

namespace Provider
{
    public class DungeonInfoPresenter
    {
        [Inject]
        public DungeonInfoPresenter(World world, DungeonInfoView dungeonInfoView)
        {
            world.ActiveMap.SubscribeToAllItemsIgnoreNull(map =>
            {
                dungeonInfoView.SetInfo(map.Name, map.Location.Level);
            });
        }
    }
}