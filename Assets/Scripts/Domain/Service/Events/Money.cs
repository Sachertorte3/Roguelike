using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Entities;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Domain.Service.Events
{
    public class Money : IDisposable, ISerializable<MoneyMemento>, IIconEventEntity
    {
        private readonly Entity _entity;
        public readonly int Amount;

        public Money(MoneyMemento data)
        {
            _entity = new Entity(data.Entity);
            Amount = data.Amount;
            Event = new PlayerEvent(
                null,
                false,
                new List<PlayerChoiceEvent>
                {
                    new PlayerChoiceEvent(
                        "拾う(選択肢としては表示されない)",
                        (player) => true,
                        (gameManager, map) => {
                            map.Player.AddMoney(Amount);
                            GameLog.Add($"{map.Player.GetName(map.Player)}は{Amount}Gを拾った");
                            map.RemoveEventEntity(this);
                            return UniTask.CompletedTask;
                        }
                    )
                }
            );
        }

        public void Dispose()
        {
            _entity.Dispose();
        }

        ~Money()
        {
            Dispose();
        }

        public Id<IEntity> Id => _entity.Id;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination, bool isThrown)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public Observable<Unit> OnDestroyed => _entity.OnDestroyed;

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

        public IEvent Event { get; init; }

        public void SetVisibility(bool visibility)
        {
            _entity.SetVisibility(visibility);
        }

        public void Destroy()
        {
            _entity.Destroy();
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
            var destination = GetThrowDestination(CurrentPosition, direction, distance, map);
            if (_entity.VisibleByPlayer.CurrentValue && destination != CurrentPosition)
            {
                _entity.SetVisibility(false);
                await map.ShowThrowAnimation(Icon, CurrentPosition, direction, distance, EntityLayer.Middle);
                _entity.Teleport(map.FindBlankPositionFrom(destination,
                    position => map.At(position).IsBlankAndStandable(EntityLayer.Bottom)));
            }
            await map.ExecuteTrapAt(destination, actor as ICharacter);
        }

        public void Teleport(Vector2Int position)
        {
            _entity.Teleport(position);
        }

        public MoneyMemento Serialize()
        {
            return new MoneyMemento
            (
                entity: _entity.Serialize(),
                amount: Amount
            );
        }

        public static MoneyMemento Build(Vector2Int position, int amount)
        {
            return new MoneyMemento(Entity.Build(position, EntityLayer.Bottom), amount);
        }
    }
}