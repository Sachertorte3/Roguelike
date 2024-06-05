#nullable enable
using Model.Domain;
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
                    if (affectionChanged.Message.Target == map.Player.Affiliation.Id)
                    {
                        var target = map.GetCharacter(affectionChanged.Message.Target);
                        if (target == null)
                            return;
                        var characterView = synchronizedCharacterView.Get(affectionChanged.Character);
                        characterView.UpdateGroupMarker(
                            target.Affiliation.IsEnemy(map.Player.Affiliation),
                            target.Affiliation.IsAlly(map.Player.Affiliation)
                        );
                    }
                });
            });
        }
    }
}