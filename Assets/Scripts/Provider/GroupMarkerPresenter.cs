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
            world.ActiveMap.SubscribeToAllItemsIgnoreNull(map =>
            {
                serialDisposable.Disposable = map.Characters.SubscribeToAllObservables(
                    character => character.Affiliation.OnAffiliationChanged,
                    (character, affectionChanged) =>
                    {
                        if (affectionChanged.Target == map.Player.Character.Affiliation.Id)
                        {
                            var characterView = synchronizedCharacterView.TryGet(character);
                            characterView?.UpdateGroupMarker(
                                character.Affiliation.IsEnemy(map.Player.Character.Affiliation),
                                character.Affiliation.IsAlly(map.Player.Character.Affiliation)
                            );
                        }
                    });
            });
        }
    }
}