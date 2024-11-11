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
using UnityEngine.AddressableAssets;
using Utilities;

namespace Domain.Service.Events
{
    public class Money : IDisposable, ISerializable<MoneyMemento>, IPlayerEventEntity, IIconEntity
    {
        public EntityBase Entity { get; init; }
        public readonly int Amount;

        public Money(MoneyMemento data)
        {
            Entity = new EntityBase(data.Entity);
            Amount = data.Amount;
            Event = new PlayerEvent(
                null,
                false,
                new List<PlayerChoiceEvent>
                {
                    new(
                        "拾う(選択肢としては表示されない)",
                        player => true,
                        (gameManager, map) =>
                        {
                            map.Player.Character.AddMoney(Amount);
                            GameLog.Add($"{map.Player.Character.GetName(map.Player)}は{Amount}Gを拾った");
                            map.RemoveEventEntity(this);
                            return UniTask.CompletedTask;
                        }
                    )
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
            <= 100 => Addressables
                .LoadAssetAsync<Sprite>("Assets/Images/icons_full_16.png[icons_full_16_362]").WaitForCompletion(),
            <= 300 => Addressables
                .LoadAssetAsync<Sprite>("Assets/Images/icons_full_16.png[icons_full_16_363]").WaitForCompletion(),
            <= 1000 => Addressables
                .LoadAssetAsync<Sprite>("Assets/Images/icons_full_16.png[icons_full_16_360]").WaitForCompletion(),
            <= 3000 => Addressables
                .LoadAssetAsync<Sprite>("Assets/Images/icons_full_16.png[icons_full_16_361]").WaitForCompletion(),
            <= 10000 => Addressables
                .LoadAssetAsync<Sprite>("Assets/Images/icons_full_16.png[icons_full_16_365]").WaitForCompletion(),
            <= 30000 => Addressables
                .LoadAssetAsync<Sprite>("Assets/Images/icons_full_16.png[icons_full_16_366]").WaitForCompletion(),
            _ => Addressables
                .LoadAssetAsync<Sprite>("Assets/Images/icons_full_16.png[icons_full_16_358]").WaitForCompletion()
        };

        public IPlayerEvent Event { get; init; }

        public void SetVisibility(bool visibility)
        {
            Entity.SetVisibility(visibility);
        }

        public void Destroy()
        {
            Entity.Destroy();
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

        public void Teleport(Vector2Int position)
        {
            Entity.Teleport(position);
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