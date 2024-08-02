#nullable enable
using Model.Game;
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
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
            {
                dungeonInfoView.SetInfo(map.Name, map.Floor);
            });
        }
    }
}