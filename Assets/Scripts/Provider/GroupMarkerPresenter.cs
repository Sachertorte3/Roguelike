#nullable enable
using Game;
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
            var serialDisposable = new SerialDisposable();
            world.ActiveMap.SubscribeToAllIgnoreNull(map =>
            {
                serialDisposable.Disposable = map.CharacterManager.CharacterEvents.OnAffiliationChanged.Subscribe(
                    affectionChanged =>
                    {
                        if (affectionChanged.Message.Target == map.Player.Affiliation.Id)
                        {
                            var characterView = synchronizedCharacterView.TryGet(affectionChanged.Character);
                            characterView?.UpdateGroupMarker(
                                affectionChanged.Character.Affiliation.IsEnemy(map.Player.Affiliation),
                                affectionChanged.Character.Affiliation.IsAlly(map.Player.Affiliation)
                            );
                        }
                    });
            });
        }
    }
}