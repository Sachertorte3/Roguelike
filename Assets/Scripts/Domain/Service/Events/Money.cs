using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Money : IDisposable, ISerializable<MoneyMemento>, IEventEntity, IIconEntity
    {
        public EntityBase Entity { get; init; }
        public readonly int Amount;

        public Money(MoneyMemento data)
        {
            Entity = new EntityBase(data.Entity);
            Amount = data.Amount;
            Event = new CharacterEvent(
                character => character.IsPlayer,
                (character, gameManager, map) =>
                {
                    map.Player.AddMoney(Amount);
                    GameLog.Add($"{map.Player.Character.GetName(map.Player)}は{Amount}Gを拾った");
                    map.RemoveEventEntity(this);
                    return UniTask.CompletedTask;
                }
            );
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        ~Money()
        {
            Dispose();
        }

        public Sprite Icon => Amount switch
        {
            <= 100 => ScriptableObjectLoader.LoadIcon("icons_full_16_362"),
            <= 300 => ScriptableObjectLoader.LoadIcon("icons_full_16_363"),
            <= 1000 => ScriptableObjectLoader.LoadIcon("icons_full_16_360"),
            <= 3000 => ScriptableObjectLoader.LoadIcon("icons_full_16_361"),
            <= 10000 => ScriptableObjectLoader.LoadIcon("icons_full_16_365"),
            <= 30000 => ScriptableObjectLoader.LoadIcon("icons_full_16_366"),
            _ => ScriptableObjectLoader.LoadIcon("icons_full_16_358")
        };

        public ICharacterEvent Event { get; init; }

        public void SetVisibility(bool visibility)
        {
            Entity.SetVisibility(visibility);
        }

        public void Destroy(string destroyLog)
        {
            Entity.Destroy(destroyLog);
        }

        public static Vector2Int GetThrowDestination(Vector2Int position, Direction8 direction, int distance, IMap map)
        {
            var result = position;

            for (var i = 0; i < distance; i++)
            {
                if (map.At(result + direction.Vector()).CanPlace(true, false, false, EntityLayer.Middle))
                {
                    result += direction.Vector();
                }
                else
                {
                    if (map.At(result + direction.Vector()).CanPlace(true, false, true, EntityLayer.Middle))
                    {
                        result += direction.Vector();
                    }

                    break;
                }
            }

            return result;
        }

        public async UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            var destination = GetThrowDestination(Entity.CurrentPosition, direction, distance, map);
            if (Entity.Visibility.CurrentValue && destination != Entity.CurrentPosition)
            {
                Entity.SetVisibility(false);
                await map.ShowThrowAnimation(Icon, Entity.CurrentPosition, direction, distance, EntityLayer.Middle);
                Entity.Teleport(map.FindBlankPositionFrom(destination,
                    position => map.At(position).IsBlankAndStandable(EntityLayer.Bottom)));
            }

            await map.ExecuteTrapAt(destination, actor as ICharacter);
        }

        public MoneyMemento Serialize()
        {
            return new MoneyMemento
            (
                Entity.Serialize(),
                Amount
            );
        }

        public static MoneyMemento Build(Vector2Int position, int amount)
        {
            return new MoneyMemento(EntityBase.Build(position, EntityLayer.Bottom), amount);
        }
    }
}