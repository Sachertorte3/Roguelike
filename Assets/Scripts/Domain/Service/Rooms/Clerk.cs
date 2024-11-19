#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Service.Events;
using Utilities;

namespace Domain.Service.Rooms
{
    public class Clerk : IPlayerEventEntity
    {
        public readonly ICharacter Character;
        public EntityBase Entity => Character.Entity;
        public IPlayerEvent Event { get; init; }

        public Clerk(ICharacter character, Func<IPlayer, bool> canExecuteEvent,
            Func<IGameManager, IMap, UniTask> doEvent)
        {
            Character = character;
            Event = new PlayerEvent(
                null,
                true,
                new List<PlayerChoiceEvent>
                {
                    new(
                        "代金を支払う",
                        canExecuteEvent,
                        (gameManager, map) => doEvent(gameManager, map)
                    )
                }
            );
        }

        public void Dispose()
        {
            Character.Dispose();
        }

        ~Clerk()
        {
            Dispose();
        }

        public void OpposingThief(ICharacter thief)
        {
            Character.Affiliation.AddForceAffiliation(thief.Entity.Id, AffiliationType.Enemy);
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return Character.BlowAway(actor, direction, distance, map);
        }
    }
}