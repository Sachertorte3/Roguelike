#nullable enable
using Domain.Service.Items;
using ObservableCollections;
using R3;

namespace Model.Game
{
    public class ThrowAnimationEntityManager
    {
        private readonly ObservableList<ThrowAnimationEntity> _throwAnimationEntities = new();
        public ThrowAnimationEntityEvents EntityEvents = new();

        public ThrowAnimationEntityManager()
        {
            EntityEvents.OnDestroyed.Subscribe(destroyed => Remove(destroyed.Entity));
        }

        public IObservableCollection<ThrowAnimationEntity> ThrowAnimationEntities => _throwAnimationEntities;

        public void Add(ThrowAnimationEntity entity)
        {
            _throwAnimationEntities.Add(entity);
            EntityEvents.Add(entity);
        }

        public void Remove(ThrowAnimationEntity entity)
        {
            _throwAnimationEntities.Remove(entity);
            EntityEvents.Remove(entity);
        }
    }
}