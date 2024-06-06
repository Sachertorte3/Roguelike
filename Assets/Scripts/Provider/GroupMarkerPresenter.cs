#nullable enable
using Model.Domain;
using Model.Game;
using R3;
using UnityEngine;
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
                        var characterView = synchronizedCharacterView.Get(affectionChanged.Character);
                        characterView.UpdateGroupMarker(
                            affectionChanged.Character.Affiliation.IsEnemy(map.Player.Affiliation),
                            affectionChanged.Character.Affiliation.IsAlly(map.Player.Affiliation)
                        );
                    }
                });
            });
        }
    }
}