#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Service.Characters.Behavior;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Rooms
{
    public class Ally : IEventEntity
    {
        public readonly ICharacter Character;
        public readonly EnemyBehavior Behavior;
        public string? ChoiceMessage => null;
        private readonly List<EntityEvent> _events;
        public IReadOnlyList<EntityEvent> Events => _events;
        public bool CanBeCanceled => true;

        public Ally(ICharacter character, EnemyBehavior behavior, IMap map)
        {
            Character = character;
            Behavior = behavior;
            _events = new()
            {
                new EntityEvent("渡す", () => Character.CanUseItem && Character.IsAlly(map.Player), async (gameManager, map) =>
                {
                    var item = await map.Player.ItemSelector.SelectItem(map.Player.Inventory);
                    if (item != null)
                    {
                        var result = character.Inventory.TryAdd(item);
                        if (result)
                        {
                            var index = map.Player.Inventory.GetItemIndex(item);
                            map.Player.ReplaceInventory(null, index);
                            GameLog.Add($"{Character.GetName(map.Player)}に{item.Name}を渡した。");
                        }
                        else
                        {
                            GameLog.Add($"{Character.GetName(map.Player)}はこれ以上アイテムを持てない。");
                        }
                    }
                }),
                new EntityEvent("一緒に行動", () => Character.IsAlly(map.Player), async (gameManager, map) =>
                {
                    Behavior.BehaviorData.PrioritizeEnemiesOverLeaders = false;
                }),
                new EntityEvent("敵優先", () => Character.IsAlly(map.Player), async (gameManager, map) =>
                {
                    Behavior.BehaviorData.PrioritizeEnemiesOverLeaders = true;
                })
            };
        }
        public void Dispose()
        {
            Character.Dispose();
        }
        ~Ally()
        {
            Dispose();
        }
        public Id<IEntity> Id => Character.Id;
        public ReadOnlyReactiveProperty<Vector2Int> Position => Character.Position;
        public Vector2Int CurrentPosition => Character.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => Character.Visibility;
        public EntityLayer Layer => Character.Layer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => Character.OnMove;
        public Observable<Vector2Int> OnTeleport => Character.OnTeleport;
        public Observable<Unit> OnDestroyed => Character.OnDestroyed;

        public void SetVisibility(bool visibility)
        {
            Character.SetVisibility(visibility);
        }

        public void Destroy()
        {
            Character.Destroy();
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return Character.BlowAway(actor, direction, distance, map);
        }

        public void Teleport(Vector2Int position)
        {
            Character.Teleport(position);
        }
    }
}