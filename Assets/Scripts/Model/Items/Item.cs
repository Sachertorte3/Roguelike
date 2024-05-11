using Cysharp.Threading.Tasks;
using ObservableCollections;
using R3;
using Scripts.Data.Area;
using Scripts.Model.Action;
using Scripts.Model.Characters.Behavior;
using Scripts.Model.Characters.Effect;
using Scripts.Model.Entities;
using Scripts.Utilities;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Scripts.Model.Items
{
    public record Item(Skill Skill)
    {
        public async UniTask Use(IActor actor, Direction8 direction)
        {
            await Skill.Use(actor, direction);
        }
        public float Evaluate(IActor actor, Direction8 direction)
        {
            return Skill.Evaluate(actor, direction);
        }
    }
    public class ItemEntity
    {
        private readonly Item _item;
        private readonly Entity _entity;
        public Vector2Int CurrentPosition => _entity.CurrentPosition;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public ItemEntity(Vector2Int spawnPosition, Item item)
        {
            _item = item;
            _entity = new Entity(spawnPosition);
        }
    }
    internal sealed class ItemFactory
    {
        public ItemEntity CreateItem(Vector2Int spawnPosition, Item item)
        {
            return new ItemEntity(spawnPosition, item);
        }
    }
    public sealed class ItemManager
    {
        private ObservableList<ItemEntity> _items = new();
        public ReadOnlyCollection<ItemEntity> Items => new(_items);
        public Observable<ItemEntity> OnItemAdded => _items.ObserveAdd().Select(item => item.Value);
        public Observable<ItemEntity> OnItemRemoved => _items.ObserveRemove().Select(item => item.Value);
        private ItemFactory _factory = new ItemFactory();
        public ItemManager()
        {

        }
        public void SpawnItem(Vector2Int spawnPosition)
        {
            _items.Add(_factory.CreateItem(spawnPosition, new Item(new Skill(10, new LineArea(2)))));
        }
    }
}
