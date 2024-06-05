#nullable enable
using Model.Game;
using R3;
using Utilities;
using VContainer;

namespace Provider
{
    public class GroupMarkerPresenter
    {
        [Inject]
        public GroupMarkerPresenter(World world, SynchronizedCharacterView synchronizedCharacterView)
        {
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
            {
                map.CharacterManager.CharacterEvents.OnAffectionChanged.Subscribe(affectionChanged =>
                {
                    if (affectionChanged.Message.Target == map.Player.Affiliation)
                    {
                        var characterView = synchronizedCharacterView.Get(affectionChanged.Character);
                        characterView.UpdateGroupMarker(affectionChanged.Message.IsEnemy, affectionChanged.Message.IsAlly);
                    }
                });
            });
        }
    }
}